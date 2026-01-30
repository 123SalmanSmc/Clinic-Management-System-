using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _context.Users
            .Include(u => u.Staff)
            .Include(u => u.Role)
            .Select(u => new UserDto(
                u.Id, 
                u.Username, 
                u.Role.Name, 
                u.RoleId, 
                u.StaffId, 
                u.Staff != null ? u.Staff.FullName : null))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _context.Users
            .Include(u => u.Staff)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
            
        if (user == null) return NotFound();

        return Ok(new UserDto(
            user.Id, 
            user.Username, 
            user.Role.Name, 
            user.RoleId, 
            user.StaffId, 
            user.Staff?.FullName));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return BadRequest("Username already exists");
        }

        // Verify Role exists
        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null) return BadRequest("Invalid Role ID");

        // Admin checks
        if (role.Name == "Admin" && dto.StaffId.HasValue)
        {
            // Optional: validation logic for Admin if needed
        }

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            StaffId = dto.StaffId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createdUser = await _context.Users
            .Include(u => u.Staff)
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id);

        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            new UserDto(createdUser.Id, createdUser.Username, createdUser.Role.Name, createdUser.RoleId, createdUser.StaffId, createdUser.Staff?.FullName));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Check if username is taken by another user
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id))
        {
            return BadRequest("Username already exists");
        }

        // Verify Role exists
        if (!await _context.Roles.AnyAsync(r => r.Id == dto.RoleId))
        {
            return BadRequest("Invalid Role ID");
        }

        user.Username = dto.Username;
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }
        user.RoleId = dto.RoleId;
        user.StaffId = dto.RoleId == 1 ? null : dto.StaffId; // Example: Admin (ID 1) shouldn't have staff ID? Or keep it flexible.

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
