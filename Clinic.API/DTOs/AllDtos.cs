namespace Clinic.API.DTOs;

// Patient DTOs
public record PatientDto(
    int Id,
    string FullName,
    string PhoneNumber,
    DateTime DateOfBirth,
    string Gender,
    string? BloodType,
    string? Email
);

public record CreatePatientDto(
    string FullName,
    string PhoneNumber,
    DateTime DateOfBirth,
    string Gender,
    string? BloodType,
    string? Email
);

public record UpdatePatientDto(
    string FullName,
    string PhoneNumber,
    DateTime DateOfBirth,
    string Gender,
    string? BloodType,
    string? Email
);

// Staff DTOs
public record StaffDto(
    int Id,
    string FullName,
    string? PhoneNumber,
    string? Gender,
    string? Email,
    DateTime? DateOfBirth,
    string? CV,
    string? Image,
    string Status,
    decimal Fee,
    string FeeType,
    int StaffTypeId,
    string? StaffTypeName,
    int? SpecializationId,
    string? SpecializationName,
    decimal? SpecializationCost // NEW: Consultant fee from Specialization
);

public record CreateStaffDto(
    string FullName,
    string? PhoneNumber,
    string? Gender,
    string? Email,
    DateTime? DateOfBirth,
    string? CV,
    string? Image,
    string Status,
    decimal Fee,
    string FeeType,
    int StaffTypeId,
    int? SpecializationId
);

public record UpdateStaffDto(
    string FullName,
    string? PhoneNumber,
    string? Gender,
    string? Email,
    DateTime? DateOfBirth,
    string? CV,
    string? Image,
    string Status,
    decimal Fee,
    string FeeType,
    int StaffTypeId,
    int? SpecializationId
);

// Specialization DTOs
public record SpecializationDto(
    int Id,
    string Name,
    decimal ConsultationCost,
    string? Description
);

public record CreateSpecializationDto(
    string Name,
    decimal ConsultationCost,
    string? Description
);

// Appointment DTOs
public record AppointmentDto(
    int Id,
    int PatientId,
    string? PatientName,
    int DoctorId,
    string? DoctorName,
    DateTime ScheduleDate,
    TimeSpan ScheduleTime,
    decimal ConsultationCost,
    decimal Discount,
    decimal VAT,
    decimal GrandTotal,
    decimal PayingAmount,
    decimal Balance
);

public record CreateAppointmentDto(
    int PatientId,
    int DoctorId,
    DateTime ScheduleDate,
    TimeSpan ScheduleTime,
    decimal ConsultationCost,
    decimal Discount,
    decimal VAT,
    decimal PayingAmount
);

public record CreateAppointmentRequestDto(
    int PatientId,
    int DoctorId,
    DateTime ScheduleDate,
    TimeSpan ScheduleTime,
    List<int> ServiceTypeIds,
    decimal Discount,
    decimal PayingAmount
);

public record CreateServiceAssignmentDto(
    int AppointmentId,
    List<int> ServiceTypeIds,
    decimal Discount,
    decimal PayingAmount
);

// Service Assignment DTOs (for viewing/listing)
public record ServiceAssignmentDto(
    int Id,
    int AppointmentId,
    int DoctorId,
    string? DoctorName,
    decimal TotalCost,
    decimal Discount,
    decimal VAT,
    decimal GrandTotal,
    decimal PayingAmount,
    decimal Balance,
    List<ServiceAssignmentDetailDto> Details
);

public record ServiceAssignmentDetailDto(
    int Id,
    int ServiceTypeId,
    string? ServiceTypeName,
    decimal Cost
);

// Medical Note DTOs
public record MedicalNoteDto(
    int Id,
    int AppointmentId,
    string? Diagnosis,
    string? Symptoms,
    string? Treatment,
    string? Prescription,
    string? Notes,
    DateTime CreatedAt
);

public record CreateMedicalNoteDto(
    int AppointmentId,
    string? Diagnosis,
    string? Symptoms,
    string? Treatment,
    string? Prescription,
    string? Notes
);

// Service DTOs
public record ServiceDto(
    int Id,
    string ServiceName,
    string? Description
);

public record CreateServiceDto(
    string ServiceName,
    string? Description
);

public record ServiceTypeDto(
    int Id,
    string ServiceTypeName,
    decimal Cost,
    string? Description,
    int ServiceId,
    string? ServiceName
);

public record CreateServiceTypeDto(
    string ServiceTypeName,
    decimal Cost,
    string? Description,
    int ServiceId
);

// Tax DTOs
public record TaxDto(
    int Id,
    string TaxName,
    string Category,
    string Type,
    bool IsDefaultTax,
    bool IsRegisteredAuthority,
    string? RegistrationNumber,
    DateTime? RegistrationDate,
    decimal Ratio, // Tax percentage (e.g., 50 for 50%)
    int Status // 1 = Active, 0 = Inactive
);

public record CreateTaxDto(
    string TaxName,
    string Category,
    string Type,
    bool IsDefaultTax,
    bool IsRegisteredAuthority,
    string? RegistrationNumber,
    DateTime? RegistrationDate,
    decimal Ratio = 0,
    int Status = 1
);

// Payment DTOs
public record PaymentDto(
    int Id,
    string PaymentScope,
    decimal Total,
    decimal Discount,
    decimal VAT,
    decimal GrandTotal,
    decimal PayingAmount,
    decimal Balance
);

public record CreatePaymentDto(
    string PaymentScope,
    decimal Total,
    decimal Discount,
    decimal VAT,
    decimal PayingAmount
);

// User DTOs
public record UserDto(
    int Id,
    string Username,
    string Role,      // Role Name
    int RoleId,       // Role ID
    int? StaffId,
    string? StaffName
);

public record CreateUserDto(
    string Username,
    string Password,
    int RoleId,       // Input is RoleId
    int? StaffId
);

// Role DTOs
public record RoleDto(int Id, string Name, string? Description);
public record CreateRoleDto(string Name, string? Description);
public record UpdateRoleDto(string Name, string? Description);

// Permission DTOs
public record PermissionDto(int Id, string Name, string RoutePath, string? Group);

// RolePermission DTOs
public record RolePermissionDto(int RoleId, int PermissionId, string PermissionName);
public record UpdateRolePermissionsDto(List<int> PermissionIds);

// Auth DTOs
public record LoginDto(string Username, string Password);
public record RegisterDto(string Username, string Password, int RoleId);
public record TokenResponse(string Token);


// Dashboard DTOs
public record DashboardStatsDto(
    int TotalPatients,
    int TotalDoctors,
    int TotalAppointmentsToday,
    decimal TotalRevenue,
    List<RecentAppointmentDto> RecentAppointments,
    List<SessionDataDto> SessionsOverview
);

public record RecentAppointmentDto(
    int Id,
    string PatientName,
    string DoctorName,
    DateTime ScheduleDate,
    TimeSpan ScheduleTime
);

public record SessionDataDto(
    string Date,
    int Count
);

public record TopSessionDto(
    string Name,
    string Type,
    string Value
);
