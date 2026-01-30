using Clinic.API.DTOs;

namespace Clinic.API.Services;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetPatientDuesAsync(int patientId);
    Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto paymentDto);
    Task<decimal> CalculateBalanceAsync(int appointmentId, int? serviceAssignmentId);
}
