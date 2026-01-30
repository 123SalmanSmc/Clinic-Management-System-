using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.Entities;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var payments = await _context.Payments
            .Select(p => new PaymentDto(p.Id, p.PaymentScope, p.Total, p.Discount, p.VAT, p.GrandTotal, p.PayingAmount, p.Balance))
            .ToListAsync();

        return Ok(payments);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        return Ok(new PaymentDto(payment.Id, payment.PaymentScope, payment.Total, payment.Discount, payment.VAT, payment.GrandTotal, payment.PayingAmount, payment.Balance));
    }

    [HttpGet("dues")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetDues()
    {
        var dues = await _context.Payments
            .Where(p => p.Balance > 0)
            .Select(p => new PaymentDto(p.Id, p.PaymentScope, p.Total, p.Discount, p.VAT, p.GrandTotal, p.PayingAmount, p.Balance))
            .ToListAsync();

        return Ok(dues);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto)
    {
        var grandTotal = dto.Total - dto.Discount + dto.VAT;
        var balance = grandTotal - dto.PayingAmount;

        var payment = new Payment
        {
            PaymentScope = dto.PaymentScope,
            Total = dto.Total,
            Discount = dto.Discount,
            VAT = dto.VAT,
            GrandTotal = grandTotal,
            PayingAmount = dto.PayingAmount,
            Balance = balance
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = payment.Id },
            new PaymentDto(payment.Id, payment.PaymentScope, payment.Total, payment.Discount, payment.VAT, payment.GrandTotal, payment.PayingAmount, payment.Balance));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CreatePaymentDto dto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        var grandTotal = dto.Total - dto.Discount + dto.VAT;
        var balance = grandTotal - dto.PayingAmount;

        payment.PaymentScope = dto.PaymentScope;
        payment.Total = dto.Total;
        payment.Discount = dto.Discount;
        payment.VAT = dto.VAT;
        payment.GrandTotal = grandTotal;
        payment.PayingAmount = dto.PayingAmount;
        payment.Balance = balance;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
