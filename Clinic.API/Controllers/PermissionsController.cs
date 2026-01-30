using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PermissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
    {
        return await _context.Permissions
            .Select(p => new PermissionDto(p.Id, p.Name, p.RoutePath, p.Group))
            .ToListAsync();
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncDestinations()
    {
        // Define system routes/permissions here
        var systemPermissions = new List<Permission>
        {
            new Permission { Name = "Dashboard", RoutePath = "/dashboard", Group = "Core" },
            new Permission { Name = "Appointments", RoutePath = "/appointments", Group = "Clinic" },
            new Permission { Name = "Patients", RoutePath = "/patients", Group = "Clinic" },
            new Permission { Name = "Doctors", RoutePath = "/doctors", Group = "Clinic" },
            new Permission { Name = "Staff", RoutePath = "/staff", Group = "Admin" },
            new Permission { Name = "Services", RoutePath = "/services", Group = "Admin" },
            new Permission { Name = "Specializations", RoutePath = "/specializations", Group = "Admin" },
            new Permission { Name = "Users", RoutePath = "/users", Group = "Admin" },
            new Permission { Name = "Roles", RoutePath = "/roles", Group = "Admin" },
            new Permission { Name = "Role Permissions", RoutePath = "/role-permissions", Group = "Admin" },
            new Permission { Name = "Tax Settings", RoutePath = "/tax-settings", Group = "Admin" }
        };

        foreach (var p in systemPermissions)
        {
            if (!await _context.Permissions.AnyAsync(x => x.RoutePath == p.RoutePath))
            {
                _context.Permissions.Add(p);
            }
        }
        await _context.SaveChangesAsync();
        return Ok("Permissions synced");
    }
}
