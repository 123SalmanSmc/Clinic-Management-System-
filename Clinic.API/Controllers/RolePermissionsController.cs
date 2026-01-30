using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/roles/{roleId}/permissions")]
[ApiController]
public class RolePermissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RolePermissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolePermissionDto>>> GetRolePermissions(int roleId)
    {
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => new RolePermissionDto(rp.RoleId, rp.PermissionId, rp.Permission.Name))
            .ToListAsync();

        return Ok(permissions);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, UpdateRolePermissionsDto dto)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null) return NotFound("Role not found");

        // Remove existing
        var existing = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
        _context.RolePermissions.RemoveRange(existing);

        // Add new
        foreach (var permId in dto.PermissionIds)
        {
            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
