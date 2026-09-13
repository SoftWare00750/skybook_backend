namespace SkyBook.Api.DTOs;

/// <summary>
/// Request for POST /api/payments/simulate. Covers all four methods since
/// only a handful of fields differ per method — the controller validates
/// which ones are required based on <see cref="Method"/>.
///
/// Pass <see cref="PaymentMethodId"/> to charge a saved payment method
/// instead of re-entering its details (card/bank fields are ignored when
/// it's set). Raw card/account numbers are accepted only to derive a
/// brand + last 4 digits for the receipt — never persisted (see
/// <see cref="Models.Payment"/> / <see cref="Models.PaymentMethod"/>).
/// </summary>
public record SimulatePaymentRequest(
    decimal Amount,
    string Method, // "card" | "bank_transfer" | "wallet" | "other"
    string? Purpose, // "flight_booking" | "wallet_topup" — defaults to "flight_booking"
    Guid? PaymentMethodId,
    string? CardNumber,
    string? CardholderName,
    string? ExpiryMonth,
    string? ExpiryYear,
    string? BankName,
    string? AccountNumber,
    string? OtherProvider
);

public record PaymentResponse(
    Guid Id,
    string Reference,
    string Method,
    string MethodLabel,
    decimal Amount,
    string Status,
    string Purpose,
    DateTime CreatedAt
);
