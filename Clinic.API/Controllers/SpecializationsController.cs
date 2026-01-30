using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SpecializationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecializationDto>>> GetAll()
    {
        var specializations = await _context.Specializations
            .Select(s => new SpecializationDto(s.Id, s.Name, s.ConsultationCost, s.Description))
            .ToListAsync();

        return Ok(specializations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpecializationDto>> GetById(int id)
    {
        var specialization = await _context.Specializations.FindAsync(id);
        if (specialization == null) return NotFound();

        return Ok(new SpecializationDto(specialization.Id, specialization.Name, specialization.ConsultationCost, specialization.Description));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SpecializationDto>> Create(CreateSpecializationDto dto)
    {
        var specialization = new Specialization
        {
            Name = dto.Name,
            ConsultationCost = dto.ConsultationCost,
            Description = dto.Description
        };

        _context.Specializations.Add(specialization);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = specialization.Id },
            new SpecializationDto(specialization.Id, specialization.Name, specialization.ConsultationCost, specialization.Description));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CreateSpecializationDto dto)
    {
        var specialization = await _context.Specializations.FindAsync(id);
        if (specialization == null) return NotFound();

        specialization.Name = dto.Name;
        specialization.ConsultationCost = dto.ConsultationCost;
        specialization.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var specialization = await _context.Specializations.FindAsync(id);
        if (specialization == null) return NotFound();

        _context.Specializations.Remove(specialization);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
