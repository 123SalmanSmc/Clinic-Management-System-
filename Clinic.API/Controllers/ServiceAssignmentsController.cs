using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceAssignmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceAssignmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateServiceAssignment(CreateServiceAssignmentDto dto)
    {
        // 1. Validate Appointment
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);
            
        if (appointment == null) return BadRequest("Appointment not found");

        // 2. Validate Services & Calculate Total
        var serviceTypes = await _context.ServiceTypes
            .Where(st => dto.ServiceTypeIds.Contains(st.Id))
            .ToListAsync();

        if (serviceTypes.Count == 0) return BadRequest("No valid services selected");

        decimal servicesTotal = serviceTypes.Sum(st => st.Cost);

        // 3. Calculate VAT
        // User Logic: "if there is row that has tax type vat and status is 1"
        // "because their cam ne 2 or more tax type vat that is status 1"
        // "if there is no tax type vat you default give 0"
        
        var activeVats = await _context.Taxes
            .Where(t => t.TaxName == "VAT" && t.Status == 1)
            .ToListAsync();

        decimal vatRate = 0m;
        foreach (var tax in activeVats)
        {
             // Use the new Ratio field which holds the percentage (e.g., 5 for 5%)
             vatRate += tax.Ratio;
        }

        decimal vatAmount = servicesTotal * (vatRate / 100m);
        decimal grandTotal = servicesTotal + vatAmount - dto.Discount;
        decimal balance = grandTotal - dto.PayingAmount;

        // 4. Save Entities Transactionally
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var assignment = new ServiceAssignment
            {
                AppointmentId = dto.AppointmentId,
                DoctorId = appointment.DoctorId, // Inherit doctor from appointment
                TotalCost = servicesTotal,
                Discount = dto.Discount,
                VAT = vatAmount,
                GrandTotal = grandTotal,
                PayingAmount = dto.PayingAmount,
                Balance = balance
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

            var payment = new Payment
            {
                PaymentScope = "Service",
                Total = servicesTotal,
                Discount = dto.Discount,
                VAT = vatAmount,
                GrandTotal = grandTotal,
                PayingAmount = dto.PayingAmount,
                Balance = balance
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Service assignment created successfully", AssignmentId = assignment.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error creating service assignment: " + ex.Message);
        }
    }

    [HttpGet("by-appointment/{appointmentId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ServiceAssignmentDto>>> GetByAppointment(int appointmentId)
    {
        var assignments = await _context.ServiceAssignments
            .Include(sa => sa.Doctor)
            .Include(sa => sa.Details)
                .ThenInclude(d => d.ServiceType)
            .Where(sa => sa.AppointmentId == appointmentId)
            .Select(sa => new ServiceAssignmentDto(
                sa.Id,
                sa.AppointmentId,
                sa.DoctorId,
                sa.Doctor != null ? sa.Doctor.FullName : null,
                sa.TotalCost,
                sa.Discount,
                sa.VAT,
                sa.GrandTotal,
                sa.PayingAmount,
                sa.Balance,
                sa.Details.Select(d => new ServiceAssignmentDetailDto(
                    d.Id,
                    d.ServiceTypeId,
                    d.ServiceType != null ? d.ServiceType.ServiceTypeName : null,
                    d.Cost
                )).ToList()
            ))
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await _context.ServiceAssignments
            .Include(sa => sa.Details)
            .FirstOrDefaultAsync(sa => sa.Id == id);

        if (assignment == null) return NotFound();

        _context.ServiceAssignmentDetails.RemoveRange(assignment.Details);
        _context.ServiceAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
