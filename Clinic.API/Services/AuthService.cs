using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinic.API.Data;
using Clinic.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Clinic.API.Services;

public interface IAuthService
{
    Task<string?> Login(string username, string password);
    Task<User> Register(string username, string password, int roleId);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(ApplicationDbContext context, PasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<string?> Login(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Username == username);

        if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    public async Task<User> Register(string username, string password, int roleId)
    {
        var passwordHash = _passwordHasher.Hash(password);
        
        // Verify Role exists
        if (!await _context.Roles.AnyAsync(r => r.Id == roleId))
        {
             throw new Exception("Invalid Role ID");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            RoleId = roleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "ClinicManagementSystemSecretKey2024VeryLongAndSecure";
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ClinicManagementSystem";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ClinicManagementSystemUsers";
        var jwtExpiresIn = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRES_IN") ?? "7");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSecret);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.Id.ToString())
        };

        if (user.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(jwtExpiresIn),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
