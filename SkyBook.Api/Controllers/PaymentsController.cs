using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Data;
using SkyBook.Api.DTOs;
using SkyBook.Api.Models;
using SkyBook.Api.Services;

namespace SkyBook.Api.Controllers;

/// <summary>
/// Simulates charging a payment method — there's no real payment gateway
/// behind this. It's used both to pay for a flight (POST /api/bookings is
/// called right after with the resulting reference) and to top up the
/// wallet (POST /api/wallet/topup follows the same way).
/// </summary>
[ApiController]
[Authorize]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(AppDbContext db, ILogger<PaymentsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // POST /api/payments/simulate
    // Always "succeeds" — after a short artificial delay so the checkout
    // flow feels real — except a wallet charge that would overdraw the
    // balance. Every attempt is logged to `payments` as a receipt; it's
    // up to the caller (bookings, wallet top-up) to decide whether the
    // wallet ledger itself changes as a result.
    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulatePaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse("Amount must be greater than zero."));
        }

        var method = request.Method.Trim().ToLowerInvariant();
        if (method is not ("card" or "bank_transfer" or "wallet" or "other"))
        {
            return BadRequest(new ErrorResponse("Method must be one of: card, bank_transfer, wallet, other."));
        }

        var userId = this.GetUserId();
        string methodLabel;

        if (request.PaymentMethodId is Guid savedId)
        {
            var saved = await _db.PaymentMethods.FirstOrDefaultAsync(p => p.Id == savedId && p.UserId == userId);
            if (saved == null) return NotFound(new ErrorResponse("Saved payment method not found."));
            methodLabel = saved.Label;
            method = saved.Type; // the saved method's own type wins over a mismatched request
        }
        else if (method == "card")
        {
            if (string.IsNullOrWhiteSpace(request.CardNumber))
                return BadRequest(new ErrorResponse("Card number is required."));
            var brand = PaymentFormatting.CardBrand(request.CardNumber);
            methodLabel = PaymentFormatting.CardLabel(brand, PaymentFormatting.Last4(request.CardNumber));
        }
        else if (method == "bank_transfer")
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                return BadRequest(new ErrorResponse("Account number is required."));
            methodLabel = PaymentFormatting.BankLabel(request.BankName, PaymentFormatting.Last4(request.AccountNumber));
        }
        else if (method == "wallet")
        {
            methodLabel = "Wallet Balance";
        }
        else // other
        {
            methodLabel = PaymentFormatting.OtherLabel(request.OtherProvider);
        }

        if (method == "wallet")
        {
            var balance = await _db.WalletTransactions
                .Where(w => w.UserId == userId)
                .SumAsync(w => (decimal?)w.Amount) ?? 0;
            if (request.Amount > balance)
            {
                return BadRequest(new ErrorResponse($"Insufficient wallet balance. Available: ${balance:0.00}."));
            }
        }

        // Simulated processing delay — long enough to feel real, short
        // enough not to make the checkout flow annoying.
        await Task.Delay(600);

        var purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "flight_booking" : request.Purpose!;
        var payment = new Payment
        {
            UserId = userId,
            Reference = PaymentFormatting.GenerateReference(),
            Method = method,
            MethodLabel = methodLabel,
            Amount = request.Amount,
            Status = "succeeded",
            Purpose = purpose,
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Simulated {Method} payment {Reference} for user {UserId}", method, payment.Reference, userId);

        return Ok(new PaymentResponse(
            payment.Id,
            payment.Reference,
            payment.Method,
            payment.MethodLabel,
            payment.Amount,
            payment.Status,
            payment.Purpose,
            payment.CreatedAt
        ));
    }

    // GET /api/payments
    // Full receipt history, newest first — used by the wallet/settings
    // screens if a user wants to see every charge (not just wallet
    // top-ups/debits, which come from GET /api/wallet instead).
    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        var userId = this.GetUserId();
        var payments = await _db.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payments.Select(p => new PaymentResponse(
            p.Id, p.Reference, p.Method, p.MethodLabel, p.Amount, p.Status, p.Purpose, p.CreatedAt
        )));
    }
}
