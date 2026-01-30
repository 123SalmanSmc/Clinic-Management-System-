using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic.API.Entities;

public class Patient
{
    public int Id { get; set; }
    [Required]
    public string FullName { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    [Required]
    public string Gender { get; set; } = string.Empty;
    public string? BloodType { get; set; }
    public string? Email { get; set; }
    // Removed Weight/Height - now in MedicalNote per user request
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class Specialization
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public decimal ConsultationCost { get; set; }
    public string? Description { get; set; }
}

public class StaffType
{
    public int Id { get; set; }
    public int IsDoctor { get; set; } // 0 = Regular, 1 = Doctor
    public string Name { get; set; } = string.Empty; // Helper for display e.g. "Doctor", "Receptionist"
}

public class Staff
{
    public int Id { get; set; }
    [Required]
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? CV { get; set; }
    public string? Image { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive
    public decimal Fee { get; set; }
    public string FeeType { get; set; } = "Fixed"; // Fixed, Percentage

    public int StaffTypeId { get; set; }
    public StaffType? StaffType { get; set; }

    public int? SpecializationId { get; set; }
    public Specialization? Specialization { get; set; }
}
