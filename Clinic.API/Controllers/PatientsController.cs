using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
    {
        return await _context.Patients
            .Select(p => new PatientDto(
                p.Id,
                p.FullName,
                p.PhoneNumber,
                p.DateOfBirth,
                p.Gender,
                p.BloodType,
                p.Email
            ))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> GetPatient(int id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound();
        }

        return new PatientDto(
            patient.Id,
            patient.FullName,
            patient.PhoneNumber,
            patient.DateOfBirth,
            patient.Gender,
            patient.BloodType,
            patient.Email
        );
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PatientDto>> PostPatient(CreatePatientDto createDto)
    {
        var patient = new Patient
        {
            FullName = createDto.FullName,
            PhoneNumber = createDto.PhoneNumber,
            DateOfBirth = createDto.DateOfBirth,
            Gender = createDto.Gender,
            BloodType = createDto.BloodType,
            Email = createDto.Email
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, new PatientDto(
            patient.Id,
            patient.FullName,
            patient.PhoneNumber,
            patient.DateOfBirth,
            patient.Gender,
            patient.BloodType,
            patient.Email
        ));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutPatient(int id, UpdatePatientDto updateDto)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();

        patient.FullName = updateDto.FullName;
        patient.PhoneNumber = updateDto.PhoneNumber;
        patient.DateOfBirth = updateDto.DateOfBirth;
        patient.Gender = updateDto.Gender;
        patient.BloodType = updateDto.BloodType;
        patient.Email = updateDto.Email;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
