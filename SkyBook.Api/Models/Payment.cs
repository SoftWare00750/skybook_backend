namespace SkyBook.Api.Models;

/// <summary>
/// A record of a (simulated) payment charge — e.g. paying for a flight
/// booking. Mirrors the "payments" table in Supabase. There's no real
/// payment gateway wired up here: <see cref="PaymentsController"/> always
/// "succeeds" after a short simulated processing step, which is enough to
/// drive a realistic checkout flow (card / bank transfer / wallet / other)
/// without handling real money.
/// </summary>
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    /// Merchant-style reference shown to the user, e.g. "PAY-7F3K9QZL".
    public string Reference { get; set; } = string.Empty;

    /// "card" | "bank_transfer" | "wallet" | "other"
    public string Method { get; set; } = string.Empty;

    /// Display label of the method used, e.g. "Visa •••• 4242".
    public string MethodLabel { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// Always "succeeded" today — kept as a string column so a future,
    /// real integration could add "failed" / "pending" without a schema
    /// change.
    public string Status { get; set; } = "succeeded";

    /// What the charge was for, e.g. "flight_booking".
    public string Purpose { get; set; } = "flight_booking";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
