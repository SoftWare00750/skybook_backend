namespace SkyBook.Api.DTOs;

public record TopUpRequest(decimal Amount, string? Label);

public record WalletTransactionResponse(Guid Id, string Label, decimal Amount, DateTime CreatedAt);

public record WalletResponse(decimal Balance, List<WalletTransactionResponse> Transactions);
