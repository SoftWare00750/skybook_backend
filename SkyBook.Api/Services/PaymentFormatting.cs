using System.Text.RegularExpressions;

namespace SkyBook.Api.Services;

/// <summary>
/// Small, stateless helpers shared by <see cref="Controllers.PaymentsController"/>
/// and <see cref="Controllers.PaymentMethodsController"/> for turning a raw
/// card/account number into the last-4-only display data this backend
/// actually persists, and for generating the merchant-style reference
/// codes shown on receipts. No real card validation (Luhn, expiry, etc.)
/// is performed — this is a simulated checkout, not a payment gateway.
/// </summary>
public static class PaymentFormatting
{
    /// Strips everything but digits, e.g. "4242 4242 4242 4242" -> "4242424242424242".
    public static string DigitsOnly(string? raw) => Regex.Replace(raw ?? string.Empty, "[^0-9]", "");

    public static string Last4(string? raw)
    {
        var digits = DigitsOnly(raw);
        return digits.Length >= 4 ? digits[^4..] : digits.PadLeft(4, '0');
    }

    /// Guesses a card brand from its leading digit(s) — the same
    /// first-digit ranges real card networks use (Visa: 4, Mastercard:
    /// 51-55/2221-2720, Amex: 34/37, Discover: 6011/65).
    public static string CardBrand(string? cardNumber)
    {
        var digits = DigitsOnly(cardNumber);
        if (digits.Length < 2) return "Card";
        if (digits.StartsWith('4')) return "Visa";
        if (digits.StartsWith("34") || digits.StartsWith("37")) return "Amex";
        if (digits.StartsWith("6011") || digits.StartsWith("65")) return "Discover";
        var firstTwo = int.Parse(digits[..2]);
        if (firstTwo is >= 51 and <= 55) return "Mastercard";
        return "Card";
    }

    public static string CardLabel(string brand, string last4) => $"{brand} •••• {last4}";

    public static string BankLabel(string? bankName, string last4) =>
        $"{(string.IsNullOrWhiteSpace(bankName) ? "Bank" : bankName)} •••• {last4}";

    public static string OtherLabel(string? provider) =>
        string.IsNullOrWhiteSpace(provider) ? "Other" : provider!;

    /// e.g. "PAY-7F3K9QZL" — merchant-style reference shown on receipts.
    public static string GenerateReference(string prefix = "PAY")
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = Random.Shared;
        var code = new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return $"{prefix}-{code}";
    }
}
