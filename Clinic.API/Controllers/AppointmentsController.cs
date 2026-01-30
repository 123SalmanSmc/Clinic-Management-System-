using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Select(a => new AppointmentDto(
                a.Id,
                a.PatientId,
                a.Patient != null ? a.Patient.FullName : null,
                a.DoctorId,
                a.Doctor != null ? a.Doctor.FullName : null,
                a.ScheduleDate,
                a.ScheduleTime,
                a.ConsultationCost,
                a.Discount,
                a.VAT,
                a.GrandTotal,
                a.PayingAmount,
                a.Balance
            ))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        return new AppointmentDto(
            appt.Id,
            appt.PatientId,
            appt.Patient?.FullName,
            appt.DoctorId,
            appt.Doctor?.FullName,
            appt.ScheduleDate,
            appt.ScheduleTime,
            appt.ConsultationCost,
            appt.Discount,
            appt.VAT,
            appt.GrandTotal,
            appt.PayingAmount,
            appt.Balance
        );
    }

    [HttpGet("today")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetTodayAppointments()
    {
        var today = DateTime.Today;
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.ScheduleDate.Date == today)
            .Select(a => new AppointmentDto(
                a.Id,
                a.PatientId,
                a.Patient != null ? a.Patient.FullName : null,
                a.DoctorId,
                a.Doctor != null ? a.Doctor.FullName : null,
                a.ScheduleDate,
                a.ScheduleTime,
                a.ConsultationCost,
                a.Discount,
                a.VAT,
                a.GrandTotal,
                a.PayingAmount,
                a.Balance
            ))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AppointmentDto>> PostAppointment(CreateAppointmentDto createDto)
    {
        // Validate Doctor - only doctors can have appointments
        var doctor = await _context.Staffs
            .Include(s => s.StaffType)
            .FirstOrDefaultAsync(s => s.Id == createDto.DoctorId);

        if (doctor == null)
        {
            return BadRequest("Doctor not found");
        }

        if (doctor.StaffType == null || doctor.StaffType.IsDoctor != 1)
        {
            return BadRequest("Only doctors can have appointments");
        }

        // Calculate GrandTotal and Balance
        var grandTotal = createDto.ConsultationCost - createDto.Discount + createDto.VAT;
        var balance = grandTotal - createDto.PayingAmount;

        var appointment = new Appointment
        {
            PatientId = createDto.PatientId,
            DoctorId = createDto.DoctorId,
            ScheduleDate = createDto.ScheduleDate,
            ScheduleTime = createDto.ScheduleTime,
            ConsultationCost = createDto.ConsultationCost,
            Discount = createDto.Discount,
            VAT = createDto.VAT,
            GrandTotal = grandTotal,
            PayingAmount = createDto.PayingAmount,
            Balance = balance
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Load relations for return DTO
        await _context.Entry(appointment).Reference(a => a.Patient).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Doctor).LoadAsync();

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, new AppointmentDto(
            appointment.Id,
            appointment.PatientId,
            appointment.Patient?.FullName,
            appointment.DoctorId,
            appointment.Doctor?.FullName,
            appointment.ScheduleDate,
            appointment.ScheduleTime,
            appointment.ConsultationCost,
            appointment.Discount,
            appointment.VAT,
            appointment.GrandTotal,
            appointment.PayingAmount,
            appointment.Balance
        ));
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<ActionResult<AppointmentDto>> SubmitAppointment(CreateAppointmentRequestDto request)
    {
        // 1. Validate Doctor & Consultant Fee
        var doctor = await _context.Staffs
            .Include(s => s.Specialization)
            .Include(s => s.StaffType)
            .FirstOrDefaultAsync(s => s.Id == request.DoctorId);

        if (doctor == null) return BadRequest("Doctor not found");
        if (doctor.StaffType?.IsDoctor != 1) return BadRequest("Selected staff is not a doctor");

        decimal consultationFee = doctor.Specialization?.ConsultationCost ?? 0m;

        // 2. Validate Services & Calculate Service Total
        var serviceTypes = await _context.ServiceTypes
            .Where(st => request.ServiceTypeIds.Contains(st.Id))
            .ToListAsync();

        decimal servicesTotal = serviceTypes.Sum(st => st.Cost);
        
        // 3. Calculate VAT
        var vatTax = await _context.Taxes
            .FirstOrDefaultAsync(t => t.TaxName == "VAT"); 
            
        decimal subTotal = consultationFee + servicesTotal;
        decimal vatRate = 0m;
        
        if (vatTax != null)
        {
             // Use the new Ratio field
             vatRate = vatTax.Ratio;
        }

        decimal vatAmount = subTotal * (vatRate / 100m);
        decimal grandTotal = subTotal + vatAmount - request.Discount;
        decimal balance = grandTotal - request.PayingAmount;

        // 4. Create Entities
        // Strategy: Use a transaction to ensure all or nothing
        using var transaction = await _context.Database.BeginTransactionAsync();
        try 
        {
            // Appointment
            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ScheduleDate = request.ScheduleDate,
                ScheduleTime = request.ScheduleTime,
                ConsultationCost = consultationFee,
                Discount = request.Discount,
                VAT = vatAmount,
                GrandTotal = grandTotal,
                PayingAmount = request.PayingAmount,
                Balance = balance
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(); // Get ID

            // Service Assignment
            var assignment = new ServiceAssignment
            {
                AppointmentId = appointment.Id,
                DoctorId = request.DoctorId,
                TotalCost = servicesTotal,
                Discount = 0, // Discount is applied on Appointment level in this logic
                VAT = vatAmount, // Could split VAT but applying here for tracking
                GrandTotal = servicesTotal + vatAmount, // keeping it simple
                PayingAmount = 0, 
                Balance = 0
            };
            
            foreach (var st in serviceTypes)
            {
                assignment.Details.Add(new ServiceAssignmentDetail
                {
                    ServiceTypeId = st.Id,
                    Cost = st.Cost
                });
            }
            
            _context.ServiceAssignments.Add(assignment);
            
            // Payment record
            var payment = new Payment
            {
                PaymentScope = "Appointment",
                Total = subTotal,
                Discount = request.Discount,
                VAT = vatAmount,
                GrandTotal = grandTotal,
                PayingAmount = request.PayingAmount,
                Balance = balance
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Load relations for return
            await _context.Entry(appointment).Reference(a => a.Patient).LoadAsync();
            await _context.Entry(appointment).Reference(a => a.Doctor).LoadAsync();

            return Ok(new AppointmentDto(
                appointment.Id,
                appointment.PatientId,
                appointment.Patient?.FullName,
                appointment.DoctorId,
                appointment.Doctor?.FullName,
                appointment.ScheduleDate,
                appointment.ScheduleTime,
                appointment.ConsultationCost,
                appointment.Discount,
                appointment.VAT,
                appointment.GrandTotal,
                appointment.PayingAmount,
                appointment.Balance
            ));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while creating the appointment: " + ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutAppointment(int id, CreateAppointmentDto updateDto)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        var grandTotal = updateDto.ConsultationCost - updateDto.Discount + updateDto.VAT;
        var balance = grandTotal - updateDto.PayingAmount;

        appointment.PatientId = updateDto.PatientId;
        appointment.DoctorId = updateDto.DoctorId;
        appointment.ScheduleDate = updateDto.ScheduleDate;
        appointment.ScheduleTime = updateDto.ScheduleTime;
        appointment.ConsultationCost = updateDto.ConsultationCost;
        appointment.Discount = updateDto.Discount;
        appointment.VAT = updateDto.VAT;
        appointment.GrandTotal = grandTotal;
        appointment.PayingAmount = updateDto.PayingAmount;
        appointment.Balance = balance;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
