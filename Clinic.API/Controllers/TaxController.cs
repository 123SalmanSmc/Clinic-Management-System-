using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TaxController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaxDto>>> GetAll()
    {
        var taxes = await _context.Taxes
            .Select(t => new TaxDto(t.Id, t.TaxName, t.Category, t.Type, t.IsDefaultTax, t.IsRegisteredAuthority, t.RegistrationNumber, t.RegistrationDate, t.Ratio, t.Status))
            .ToListAsync();

        return Ok(taxes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaxDto>> GetById(int id)
    {
        var tax = await _context.Taxes.FindAsync(id);
        if (tax == null) return NotFound();

        return Ok(new TaxDto(tax.Id, tax.TaxName, tax.Category, tax.Type, tax.IsDefaultTax, tax.IsRegisteredAuthority, tax.RegistrationNumber, tax.RegistrationDate, tax.Ratio, tax.Status));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TaxDto>> Create(CreateTaxDto dto)
    {
        var tax = new Tax
        {
            TaxName = dto.TaxName,
            Category = dto.Category,
            Type = dto.Type,
            IsDefaultTax = dto.IsDefaultTax,
            IsRegisteredAuthority = dto.IsRegisteredAuthority,
            RegistrationNumber = dto.RegistrationNumber,
            RegistrationDate = dto.RegistrationDate,
            Ratio = dto.Ratio,
            Status = dto.Status
        };

        _context.Taxes.Add(tax);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tax.Id },
            new TaxDto(tax.Id, tax.TaxName, tax.Category, tax.Type, tax.IsDefaultTax, tax.IsRegisteredAuthority, tax.RegistrationNumber, tax.RegistrationDate, tax.Ratio, tax.Status));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CreateTaxDto dto)
    {
        var tax = await _context.Taxes.FindAsync(id);
        if (tax == null) return NotFound();

        tax.TaxName = dto.TaxName;
        tax.Category = dto.Category;
        tax.Type = dto.Type;
        tax.IsDefaultTax = dto.IsDefaultTax;
        tax.IsRegisteredAuthority = dto.IsRegisteredAuthority;
        tax.RegistrationNumber = dto.RegistrationNumber;
        tax.RegistrationDate = dto.RegistrationDate;
        tax.Ratio = dto.Ratio;
        tax.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var tax = await _context.Taxes.FindAsync(id);
        if (tax == null) return NotFound();

        _context.Taxes.Remove(tax);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("default")]
    public async Task<ActionResult<TaxDto?>> GetDefaultTax()
    {
        var tax = await _context.Taxes.FirstOrDefaultAsync(t => t.IsDefaultTax);
        if (tax == null) return Ok(null);

        return Ok(new TaxDto(tax.Id, tax.TaxName, tax.Category, tax.Type, tax.IsDefaultTax, tax.IsRegisteredAuthority, tax.RegistrationNumber, tax.RegistrationDate, tax.Ratio, tax.Status));
    }
}
