using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StaffController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StaffController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetStaff()
    {
        return await _context.Staffs
            .Include(s => s.StaffType)
            .Include(s => s.Specialization)
            .Select(s => new StaffDto(
                s.Id,
                s.FullName,
                s.PhoneNumber,
                s.Gender,
                s.Email,
                s.DateOfBirth,
                s.CV,
                s.Image,
                s.Status,
                s.Fee,
                s.FeeType,
                s.StaffTypeId,
                s.StaffType != null ? s.StaffType.Name : null,
                s.SpecializationId,
                s.Specialization != null ? s.Specialization.Name : null,
                s.Specialization != null ? s.Specialization.ConsultationCost : null
            ))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StaffDto>> GetStaffById(int id)
    {
        var staff = await _context.Staffs
            .Include(s => s.StaffType)
            .Include(s => s.Specialization)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (staff == null) return NotFound();

        return new StaffDto(
            staff.Id,
            staff.FullName,
            staff.PhoneNumber,
            staff.Gender,
            staff.Email,
            staff.DateOfBirth,
            staff.CV,
            staff.Image,
            staff.Status,
            staff.Fee,
            staff.FeeType,
            staff.StaffTypeId,
            staff.StaffType?.Name,
            staff.SpecializationId,
            staff.Specialization?.Name,
            staff.Specialization?.ConsultationCost
        );
    }

    [HttpGet("doctors")]
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetDoctors()
    {
        return await _context.Staffs
            .Include(s => s.StaffType)
            .Include(s => s.Specialization)
            .Where(s => s.StaffType != null && s.StaffType.IsDoctor == 1)
            .Select(s => new StaffDto(
                s.Id,
                s.FullName,
                s.PhoneNumber,
                s.Gender,
                s.Email,
                s.DateOfBirth,
                s.CV,
                s.Image,
                s.Status,
                s.Fee,
                s.FeeType,
                s.StaffTypeId,
                s.StaffType != null ? s.StaffType.Name : null,
                s.SpecializationId,
                s.Specialization != null ? s.Specialization.Name : null,
                s.Specialization != null ? s.Specialization.ConsultationCost : null
            ))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<StaffDto>> PostStaff(CreateStaffDto createDto)
    {
        var staff = new Staff
        {
            FullName = createDto.FullName,
            PhoneNumber = createDto.PhoneNumber,
            Gender = createDto.Gender,
            Email = createDto.Email,
            DateOfBirth = createDto.DateOfBirth,
            CV = createDto.CV,
            Image = createDto.Image,
            Status = createDto.Status,
            Fee = createDto.Fee,
            FeeType = createDto.FeeType,
            StaffTypeId = createDto.StaffTypeId,
            SpecializationId = createDto.SpecializationId
        };
        
        _context.Staffs.Add(staff);
        await _context.SaveChangesAsync();

        var staffType = await _context.StaffTypes.FindAsync(createDto.StaffTypeId);
        var specialization = createDto.SpecializationId.HasValue 
            ? await _context.Specializations.FindAsync(createDto.SpecializationId) 
            : null;
        
        return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id }, new StaffDto(
            staff.Id,
            staff.FullName,
            staff.PhoneNumber,
            staff.Gender,
            staff.Email,
            staff.DateOfBirth,
            staff.CV,
            staff.Image,
            staff.Status,
            staff.Fee,
            staff.FeeType,
            staff.StaffTypeId,
            staffType?.Name,
            staff.SpecializationId,
            specialization?.Name,
            specialization?.ConsultationCost
        ));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutStaff(int id, UpdateStaffDto updateDto)
    {
        var staff = await _context.Staffs.FindAsync(id);
        if (staff == null) return NotFound();

        staff.FullName = updateDto.FullName;
        staff.PhoneNumber = updateDto.PhoneNumber;
        staff.Gender = updateDto.Gender;
        staff.Email = updateDto.Email;
        staff.DateOfBirth = updateDto.DateOfBirth;
        staff.CV = updateDto.CV;
        staff.Image = updateDto.Image;
        staff.Status = updateDto.Status;
        staff.Fee = updateDto.Fee;
        staff.FeeType = updateDto.FeeType;
        staff.StaffTypeId = updateDto.StaffTypeId;
        staff.SpecializationId = updateDto.SpecializationId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var staff = await _context.Staffs.FindAsync(id);
        if (staff == null) return NotFound();

        _context.Staffs.Remove(staff);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<object>>> GetStaffTypes()
    {
        return Ok(await _context.StaffTypes.ToListAsync());
    }
}
