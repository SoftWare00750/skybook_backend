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
            transactions.Select(t => new WalletTransactionResponse(t.Id, t.Label, t.Amount, t.CreatedAt)).ToList()
        ));
    }

    // POST /api/wallet/topup
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse("Top-up amount must be greater than zero."));
        }

        var userId = this.GetUserId();
        var transaction = new WalletTransaction
        {
            UserId = userId,
            Label = string.IsNullOrWhiteSpace(request.Label) ? "Money Added" : request.Label!,
            Amount = request.Amount,
        };

        _db.WalletTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new WalletTransactionResponse(transaction.Id, transaction.Label, transaction.Amount, transaction.CreatedAt));
    }
}
