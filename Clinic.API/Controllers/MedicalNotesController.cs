using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MedicalNotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MedicalNotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-appointment/{appointmentId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<MedicalNoteDto>>> GetByAppointment(int appointmentId)
    {
        var notes = await _context.MedicalNotes
            .Where(n => n.AppointmentId == appointmentId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new MedicalNoteDto(
                n.Id,
                n.AppointmentId,
                n.Diagnosis,
                n.Symptoms,
                n.Treatment,
                n.Prescription,
                n.Notes,
                n.CreatedAt
            ))
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MedicalNoteDto>> Create(CreateMedicalNoteDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
        if (appointment == null) return BadRequest("Appointment not found");

        var note = new MedicalNote
        {
            AppointmentId = dto.AppointmentId,
            Diagnosis = dto.Diagnosis,
            Symptoms = dto.Symptoms,
            Treatment = dto.Treatment,
            Prescription = dto.Prescription,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.MedicalNotes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByAppointment), new { appointmentId = note.AppointmentId },
            new MedicalNoteDto(note.Id, note.AppointmentId, note.Diagnosis, note.Symptoms, note.Treatment, note.Prescription, note.Notes, note.CreatedAt));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CreateMedicalNoteDto dto)
    {
        var note = await _context.MedicalNotes.FindAsync(id);
        if (note == null) return NotFound();

        note.Diagnosis = dto.Diagnosis;
        note.Symptoms = dto.Symptoms;
        note.Treatment = dto.Treatment;
        note.Prescription = dto.Prescription;
        note.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _context.MedicalNotes.FindAsync(id);
        if (note == null) return NotFound();

        _context.MedicalNotes.Remove(note);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
