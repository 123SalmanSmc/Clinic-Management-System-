using Clinic.API.Data;
using Clinic.API.DTOs;
using Clinic.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.API.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentDto>> GetPatientDuesAsync(int patientId)
    {
        var duePayments = new List<PaymentDto>();

        // Get Appointment Dues
        var appointments = await _context.Appointments
            .Where(a => a.PatientId == patientId && a.Balance > 0)
            .Include(a => a.Doctor)
            .ToListAsync();

        foreach (var appt in appointments)
        {
            duePayments.Add(new PaymentDto(
                appt.Id,
                "Appointment",
                appt.ConsultationCost,
                appt.Discount,
                appt.VAT,
                appt.GrandTotal,
                appt.PayingAmount,
                appt.Balance
            ));
        }

        // Get Service Assignment Dues via Appointment
        var serviceAssignments = await _context.ServiceAssignments
            .Include(sa => sa.Appointment)
            .Where(sa => sa.Appointment != null && sa.Appointment.PatientId == patientId && sa.Balance > 0)
            .ToListAsync();

        foreach (var sa in serviceAssignments)
        {
            duePayments.Add(new PaymentDto(
                sa.Id,
                "Service",
                sa.TotalCost,
                sa.Discount,
                sa.VAT,
                sa.GrandTotal,
                sa.PayingAmount,
                sa.Balance
            ));
        }

        return duePayments;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto paymentDto)
    {
        var grandTotal = paymentDto.Total - paymentDto.Discount + paymentDto.VAT;
        var balance = grandTotal - paymentDto.PayingAmount;

        var payment = new Payment
        {
            PaymentScope = paymentDto.PaymentScope,
            Total = paymentDto.Total,
            Discount = paymentDto.Discount,
            VAT = paymentDto.VAT,
            GrandTotal = grandTotal,
            PayingAmount = paymentDto.PayingAmount,
            Balance = balance
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentDto(
            payment.Id,
            payment.PaymentScope,
            payment.Total,
            payment.Discount,
            payment.VAT,
            payment.GrandTotal,
            payment.PayingAmount,
            payment.Balance
        );
    }

    public Task<decimal> CalculateBalanceAsync(int appointmentId, int? serviceAssignmentId)
    {
        throw new NotImplementedException();
    }
}
