using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServices()
    {
        return await _context.Services
            .Select(s => new ServiceDto(
                s.Id,
                s.ServiceName,
                s.Description
            ))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceDto>> GetService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        return new ServiceDto(service.Id, service.ServiceName, service.Description);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ServiceDto>> PostService(CreateServiceDto createDto)
    {
        var service = new Service
        {
            ServiceName = createDto.ServiceName,
            Description = createDto.Description
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetService), new { id = service.Id }, 
            new ServiceDto(service.Id, service.ServiceName, service.Description));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutService(int id, CreateServiceDto updateDto)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        service.ServiceName = updateDto.ServiceName;
        service.Description = updateDto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
