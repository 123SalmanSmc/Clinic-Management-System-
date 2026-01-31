using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Clinic.API.Entities;

public class User
{
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    // Changed from string Role to FK
    // public string Role { get; set; } = "User"; 

    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }
}

public class Role
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty; // Admin, Doctor, Receptionist
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty; // e.g. "View Appointments"
    [Required]
    public string RoutePath { get; set; } = string.Empty; // e.g. "/appointments"
    public string? Group { get; set; } // e.g. "Appointments"
}

public class RolePermission
{
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int DoctorId { get; set; } // Points to Staff with IsDoctor=1
    public Staff? Doctor { get; set; }

    public DateTime ScheduleDate { get; set; }
    public TimeSpan ScheduleTime { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ConsultationCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VAT { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PayingAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
}

public class Tax
{
    public int Id { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = "Percentage"; // Symbol or Percentage
    public bool IsDefaultTax { get; set; }
    public bool IsRegisteredAuthority { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public int Status { get; set; } = 1; // 1 = Active, 0 = Inactive
    [Precision(18, 4)] 
    public decimal Ratio { get; set; } = 0; // Tax percentage, e.g., 15 for 15%
}

public class Service
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}

public class ServiceType
{
    public int Id { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }
    public string? Description { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }
}

public class ServiceAssignment
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int DoctorId { get; set; }
    public Staff? Doctor { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VAT { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PayingAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    public ICollection<ServiceAssignmentDetail> Details { get; set; } = new List<ServiceAssignmentDetail>();
}

public class ServiceAssignmentDetail
{
    public int Id { get; set; }
    public int ServiceAssignmentId { get; set; }
    public ServiceAssignment? ServiceAssignment { get; set; } // Navigation property

    public int ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public string PaymentScope { get; set; } = "All"; // All, Appointment, Service

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VAT { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PayingAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
}

// Medical Note linked to Appointment (not Patient)
public class MedicalNote
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public string? Diagnosis { get; set; }
    public string? Symptoms { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
