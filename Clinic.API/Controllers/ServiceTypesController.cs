using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceTypesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceTypeDto>>> GetAll()
    {
        var serviceTypes = await _context.ServiceTypes
            .Include(st => st.Service)
            .Select(st => new ServiceTypeDto(
                st.Id, 
                st.ServiceTypeName, 
                st.Cost, 
                st.Description, 
                st.ServiceId, 
                st.Service != null ? st.Service.ServiceName : null
            ))
            .ToListAsync();

        return Ok(serviceTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceTypeDto>> GetById(int id)
    {
        var serviceType = await _context.ServiceTypes
            .Include(st => st.Service)
            .FirstOrDefaultAsync(st => st.Id == id);
            
        if (serviceType == null) return NotFound();

        return Ok(new ServiceTypeDto(
            serviceType.Id, 
            serviceType.ServiceTypeName, 
            serviceType.Cost, 
            serviceType.Description, 
            serviceType.ServiceId, 
            serviceType.Service?.ServiceName
        ));
    }

    [HttpGet("by-service/{serviceId}")]
    public async Task<ActionResult<IEnumerable<ServiceTypeDto>>> GetByService(int serviceId)
    {
        var serviceTypes = await _context.ServiceTypes
            .Include(st => st.Service)
            .Where(st => st.ServiceId == serviceId)
            .Select(st => new ServiceTypeDto(
                st.Id, 
                st.ServiceTypeName, 
                st.Cost, 
                st.Description, 
                st.ServiceId, 
                st.Service != null ? st.Service.ServiceName : null
            ))
            .ToListAsync();

        return Ok(serviceTypes);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ServiceTypeDto>> Create(CreateServiceTypeDto dto)
    {
        var serviceType = new ServiceType
        {
            ServiceTypeName = dto.ServiceTypeName,
            Cost = dto.Cost,
            Description = dto.Description,
            ServiceId = dto.ServiceId
        };

        _context.ServiceTypes.Add(serviceType);
        await _context.SaveChangesAsync();

        var service = await _context.Services.FindAsync(dto.ServiceId);
        return CreatedAtAction(nameof(GetById), new { id = serviceType.Id },
            new ServiceTypeDto(serviceType.Id, serviceType.ServiceTypeName, serviceType.Cost, serviceType.Description, serviceType.ServiceId, service?.ServiceName));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CreateServiceTypeDto dto)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType == null) return NotFound();

        serviceType.ServiceTypeName = dto.ServiceTypeName;
        serviceType.Cost = dto.Cost;
        serviceType.Description = dto.Description;
        serviceType.ServiceId = dto.ServiceId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType == null) return NotFound();

        _context.ServiceTypes.Remove(serviceType);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
