using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Data;
using SkyBook.Api.DTOs;
using SkyBook.Api.Models;
using SkyBook.Api.Services;

namespace SkyBook.Api.Controllers;

/// <summary>
/// CRUD for the payment methods a user has saved to their wallet, so
/// checkout (flight payment or a wallet top-up) can offer "pay with a
/// saved card" instead of re-entering details every time.
/// </summary>
[ApiController]
[Authorize]
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentMethodsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/payment-methods
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var userId = this.GetUserId();
        var methods = await _db.PaymentMethods
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(methods.Select(ToResponse));
    }

    // POST /api/payment-methods
    // Card/account numbers are accepted only to derive a brand + last 4
    // digits — the raw value is never persisted (see PaymentMethod.cs).
    [HttpPost]
    public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
    {
        var type = request.Type?.Trim().ToLowerInvariant();
        if (type is not ("card" or "bank_transfer" or "other"))
        {
            return BadRequest(new ErrorResponse("Type must be one of: card, bank_transfer, other."));
        }

        var userId = this.GetUserId();
        var method = new PaymentMethod { UserId = userId, Type = type };

        switch (type)
        {
            case "card":
                if (string.IsNullOrWhiteSpace(request.CardNumber))
                    return BadRequest(new ErrorResponse("Card number is required."));
                method.Brand = PaymentFormatting.CardBrand(request.CardNumber);
                method.Last4 = PaymentFormatting.Last4(request.CardNumber);
                method.ExpiryMonth = request.ExpiryMonth;
                method.ExpiryYear = request.ExpiryYear;
                method.Label = PaymentFormatting.CardLabel(method.Brand, method.Last4);
                break;
            case "bank_transfer":
                if (string.IsNullOrWhiteSpace(request.AccountNumber))
                    return BadRequest(new ErrorResponse("Account number is required."));
                method.BankName = request.BankName;
                method.AccountLast4 = PaymentFormatting.Last4(request.AccountNumber);
                method.Label = PaymentFormatting.BankLabel(method.BankName, method.AccountLast4);
                break;
            default: // other
                if (string.IsNullOrWhiteSpace(request.OtherProvider))
                    return BadRequest(new ErrorResponse("Provider name is required."));
                method.OtherProvider = request.OtherProvider;
                method.Label = PaymentFormatting.OtherLabel(method.OtherProvider);
                break;
        }

        var isFirst = !await _db.PaymentMethods.AnyAsync(p => p.UserId == userId);
        method.IsDefault = request.SetDefault || isFirst;

        if (method.IsDefault)
        {
            await UnsetOtherDefaults(userId, keep: null);
        }

        _db.PaymentMethods.Add(method);
        await _db.SaveChangesAsync();

        return Created(string.Empty, ToResponse(method));
    }

    // PUT /api/payment-methods/{id}/default
    [HttpPut("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var userId = this.GetUserId();
        var method = await _db.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (method == null) return NotFound(new ErrorResponse("Payment method not found."));

        await UnsetOtherDefaults(userId, keep: id);
        method.IsDefault = true;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(method));
    }

    // DELETE /api/payment-methods/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePaymentMethod(Guid id)
    {
        var userId = this.GetUserId();
        var method = await _db.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (method == null) return NotFound(new ErrorResponse("Payment method not found."));

        var wasDefault = method.IsDefault;
        _db.PaymentMethods.Remove(method);
        await _db.SaveChangesAsync();

        // If the default method was removed, promote the next most recent
        // one so there's always an obvious "pay with" choice when one exists.
        if (wasDefault)
        {
            var next = await _db.PaymentMethods
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
            if (next != null)
            {
                next.IsDefault = true;
                await _db.SaveChangesAsync();
            }
        }

        return NoContent();
    }

    private async Task UnsetOtherDefaults(Guid userId, Guid? keep)
    {
        var others = await _db.PaymentMethods
            .Where(p => p.UserId == userId && p.IsDefault && p.Id != keep)
            .ToListAsync();
        foreach (var m in others) m.IsDefault = false;
    }

    private static PaymentMethodResponse ToResponse(PaymentMethod p) => new(
        p.Id, p.Type, p.Label, p.Brand, p.Last4, p.ExpiryMonth, p.ExpiryYear,
        p.BankName, p.AccountLast4, p.OtherProvider, p.IsDefault, p.CreatedAt
    );
}
