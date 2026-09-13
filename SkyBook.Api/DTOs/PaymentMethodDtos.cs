namespace SkyBook.Api.DTOs;

/// <summary>
/// One request shape covers all three types since only a handful of
/// fields differ — the controller validates which ones are required based
/// on <see cref="Type"/>. Card numbers / account numbers are accepted here
/// only so we can derive a brand + last 4 digits; the raw value is never
/// persisted (see <see cref="Models.PaymentMethod"/>).
/// </summary>
public record AddPaymentMethodRequest(
    string Type, // "card" | "bank_transfer" | "other"
    string? CardNumber,
    string? CardholderName,
    string? ExpiryMonth,
    string? ExpiryYear,
    string? BankName,
    string? AccountNumber,
    string? OtherProvider,
    bool SetDefault
);

public record PaymentMethodResponse(
    Guid Id,
    string Type,
    string Label,
    string? Brand,
    string? Last4,
    string? ExpiryMonth,
    string? ExpiryYear,
    string? BankName,
    string? AccountLast4,
    string? OtherProvider,
    bool IsDefault,
    DateTime CreatedAt
);
