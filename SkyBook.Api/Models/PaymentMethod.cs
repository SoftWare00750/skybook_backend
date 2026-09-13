namespace SkyBook.Api.Models;

/// <summary>
/// A payment method saved to a user's SkyBook wallet (Settings/Wallet ->
/// "Payment Methods"), so checkout can offer "pay with a saved card"
/// instead of re-entering details every time. Mirrors the
/// "payment_methods" table in Supabase.
///
/// This backend never stores a full card number or bank account number —
/// only the last 4 digits, matching how a real payment processor's
/// tokenized reference would be handled (the raw PAN would live with the
/// processor, never in our own database).
/// </summary>
public class PaymentMethod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    /// "card" | "bank_transfer" | "other"
    public string Type { get; set; } = "card";

    /// Display label shown in the UI, e.g. "Visa •••• 4242",
    /// "Chase Bank •••• 1234", or "PayPal".
    public string Label { get; set; } = string.Empty;

    // -- card-specific fields (null for other types) ------------------
    public string? Brand { get; set; } // "Visa", "Mastercard", "Amex", ...
    public string? Last4 { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }

    // -- bank_transfer-specific fields ---------------------------------
    public string? BankName { get; set; }
    public string? AccountLast4 { get; set; }

    // -- other-specific fields -------------------------------------------
    public string? OtherProvider { get; set; } // e.g. "PayPal", "Apple Pay"

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
