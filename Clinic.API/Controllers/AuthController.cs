using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.DTOs;
using Clinic.API.Services;
using Clinic.API.Entities;
using System.Security.Claims;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;

    public AuthController(IAuthService authService, ApplicationDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        var token = await _authService.Login(loginDto.Username, loginDto.Password);
        if (token == null)
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Get JWT expiration from environment
        var jwtExpiresIn = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRES_IN") ?? "7");

        // Set JWT token in HTTP-only secure cookie
        var updatedCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only secure if request is HTTPS
            SameSite = SameSiteMode.None, // Better compatibility for localhost
            Expires = DateTimeOffset.UtcNow.AddDays(jwtExpiresIn),
            Path = "/"
        };
        
        Response.Cookies.Append("jwt", token, updatedCookieOptions);

        return Ok(new { Message = "Login successful" });
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            var user = await _authService.Register(registerDto.Username, registerDto.Password, registerDto.RoleId);
            return Ok(new { Message = "User registered successfully", UserId = user.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public ActionResult Logout()
    {
        // Clear the JWT cookie
        Response.Cookies.Append("jwt", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Set to true in production with HTTPS
            SameSite = SameSiteMode.None, // Better compatibility for localhost
            Expires = DateTimeOffset.UtcNow.AddDays(-1), // Expire immediately
            Path = "/"
        });

        return Ok(new { Message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        // userId from claim
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        int userId = int.Parse(userIdClaim);

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Staff)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null) return NotFound();

        // Get Permissions
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.RoutePath)
            .ToListAsync();

        return Ok(new { 
            Username = user.Username, 
            Role = user.Role?.Name, 
            RoleId = user.RoleId,
            UserId = user.Id,
            StaffId = user.StaffId,
            StaffName = user.Staff?.FullName,
            Permissions = permissions
        });
    }
}
