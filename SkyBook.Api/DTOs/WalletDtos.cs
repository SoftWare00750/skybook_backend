namespace SkyBook.Api.DTOs;

public record TopUpRequest(
    decimal Amount,
    string? Label,
    // How the top-up was funded, from the POST /api/payments/simulate
    // response that should have just run on the client. "card" |
    // "bank_transfer" | "other" — top-ups can't be funded "from wallet".
    string? Method,
    string? MethodLabel,
    string? Reference
);

public record WalletTransactionResponse(
    Guid Id,
    string Label,
    decimal Amount,
    string? Method,
    string? Reference,
    DateTime CreatedAt
);

public record WalletResponse(decimal Balance, List<WalletTransactionResponse> Transactions);
