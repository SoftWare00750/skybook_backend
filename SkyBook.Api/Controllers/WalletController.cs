using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Data;
using SkyBook.Api.DTOs;
using SkyBook.Api.Models;
using SkyBook.Api.Services;

namespace SkyBook.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _db;

    public WalletController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/wallet
    // Balance is always derived by summing the ledger rather than stored,
    // so it can never drift out of sync with the transaction history.
    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId = this.GetUserId();
        var transactions = await _db.WalletTransactions
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        var balance = transactions.Sum(t => t.Amount);

        return Ok(new WalletResponse(
            balance,
            transactions.Select(t => new WalletTransactionResponse(t.Id, t.Label, t.Amount, t.Method, t.Reference, t.CreatedAt)).ToList()
        ));
    }

    // POST /api/wallet/topup
    // Called once POST /api/payments/simulate has charged a card / bank
    // transfer / other method for the same amount, so this just records
    // the resulting credit on the wallet ledger.
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse("Top-up amount must be greater than zero."));
        }

        var userId = this.GetUserId();
        var label = string.IsNullOrWhiteSpace(request.Label)
            ? (string.IsNullOrWhiteSpace(request.MethodLabel) ? "Money Added" : $"Money Added · {request.MethodLabel}")
            : request.Label!;

        var transaction = new WalletTransaction
        {
            UserId = userId,
            Label = label,
            Amount = request.Amount,
            Method = request.Method,
            Reference = request.Reference,
        };

        _db.WalletTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new WalletTransactionResponse(
            transaction.Id, transaction.Label, transaction.Amount, transaction.Method, transaction.Reference, transaction.CreatedAt
        ));
    }
}
