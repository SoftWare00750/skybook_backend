namespace SkyBook.Api.Models;

/// <summary>
/// A single wallet ledger entry for a user. Mirrors the
/// "wallet_transactions" table in Supabase. A user's balance is always
/// computed as the sum of their transactions' <see cref="Amount"/> rather
/// than stored separately, so the ledger and the balance can never
/// disagree.
/// </summary>
public class WalletTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public string Label { get; set; } = string.Empty; // e.g. "Flight Booking", "Money Added"

    /// Positive = money added to the wallet, negative = money spent.
    public decimal Amount { get; set; }

    /// How this ledger entry was funded/spent, when it involved an
    /// external payment: "card" | "bank_transfer" | "other" for top-ups,
    /// "wallet" for a flight paid straight out of the wallet balance, or
    /// null for entries with no associated payment (kept for older rows).
    public string? Method { get; set; }

    /// The simulated payment's reference code (see Payment.Reference),
    /// when this entry was created alongside a POST /api/payments/simulate
    /// call. Null for entries that don't have one.
    public string? Reference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
