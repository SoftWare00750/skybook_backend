namespace SkyBook.Api.DTOs;

public record SignupRequest(string FullName, string Email, string? Phone, string Password);

public record LoginRequest(string EmailOrPhone, string Password);

public record AuthResponse(string Token, string FullName, string Email);

public record ErrorResponse(string Message);
