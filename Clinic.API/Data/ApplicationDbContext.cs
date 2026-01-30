using Microsoft.EntityFrameworkCore;
using Clinic.API.Entities;

namespace Clinic.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<MedicalNote> MedicalNotes { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<StaffType> StaffTypes { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<ServiceAssignment> ServiceAssignments { get; set; }
    public DbSet<ServiceAssignmentDetail> ServiceAssignmentDetails { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Decimal Precision for Money fields
        // Specialization
        modelBuilder.Entity<Specialization>().Property(s => s.ConsultationCost).HasColumnType("decimal(18,2)");

        // Staff
        modelBuilder.Entity<Staff>().Property(s => s.Fee).HasColumnType("decimal(18,2)");
        
        // Appointment
        modelBuilder.Entity<Appointment>().Property(a => a.ConsultationCost).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Appointment>().Property(a => a.Discount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Appointment>().Property(a => a.VAT).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Appointment>().Property(a => a.GrandTotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Appointment>().Property(a => a.PayingAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Appointment>().Property(a => a.Balance).HasColumnType("decimal(18,2)");

        // ServiceType
        modelBuilder.Entity<ServiceType>().Property(s => s.Cost).HasColumnType("decimal(18,2)");

        // ServiceAssignment
        modelBuilder.Entity<ServiceAssignment>().Property(s => s.TotalCost).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceAssignment>().Property(s => s.Discount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceAssignment>().Property(s => s.VAT).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceAssignment>().Property(s => s.GrandTotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceAssignment>().Property(s => s.PayingAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceAssignment>().Property(s => s.Balance).HasColumnType("decimal(18,2)");

        // ServiceAssignmentDetail
        modelBuilder.Entity<ServiceAssignmentDetail>().Property(s => s.Cost).HasColumnType("decimal(18,2)");

        // Payment
        modelBuilder.Entity<Payment>().Property(p => p.Total).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payment>().Property(p => p.Discount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payment>().Property(p => p.VAT).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payment>().Property(p => p.GrandTotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payment>().Property(p => p.PayingAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payment>().Property(p => p.Balance).HasColumnType("decimal(18,2)");

        // Relationships

        // Staff -> StaffType
        modelBuilder.Entity<Staff>()
            .HasOne(s => s.StaffType)
            .WithMany()
            .HasForeignKey(s => s.StaffTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Staff -> Specialization (Optional)
        modelBuilder.Entity<Staff>()
            .HasOne(s => s.Specialization)
            .WithMany()
            .HasForeignKey(s => s.SpecializationId)
            .OnDelete(DeleteBehavior.SetNull);

        // User -> Staff (Optional One-to-One mostly)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Staff)
            .WithMany()
            .HasForeignKey(u => u.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment -> Patient
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting patient if they have appointments

        // Appointment -> Doctor (Staff)
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ServiceAssignment -> Appointment
        modelBuilder.Entity<ServiceAssignment>()
            .HasOne(sa => sa.Appointment)
            .WithMany()
            .HasForeignKey(sa => sa.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade); // Start with Cascade, refine if needed

        // ServiceAssignment -> Doctor
        modelBuilder.Entity<ServiceAssignment>()
            .HasOne(sa => sa.Doctor)
            .WithMany()
            .HasForeignKey(sa => sa.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // User -> Role
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // RolePermission Composite Key
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId);
            
        // Initial Data Seeding
        modelBuilder.Entity<StaffType>().HasData(
            new StaffType { Id = 1, Name = "Doctor", IsDoctor = 1 },
            new StaffType { Id = 2, Name = "Staff", IsDoctor = 0 }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Full access" },
            new Role { Id = 2, Name = "Doctor", Description = "Access to appointments and patients" },
            new Role { Id = 3, Name = "Staff", Description = "Limited access" }
        );

        modelBuilder.Entity<Tax>().HasData(
            new Tax { Id = 1, TaxName = "VAT", Category = "General", Type = "Percentage", IsDefaultTax = true, IsRegisteredAuthority = false }
        );

        // Seed Admin
        // Username: admin
        // Password: 123
        // RoleId: 1 (Admin)
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                PasswordHash = "1BDF7BABB5FB79DD23C5E743207594513236923AC552132A3CE334637215DAE0-E5D489B2753578736047D93A8AB484BF", 
                RoleId = 1 
            }
        );
    }
}
