namespace SkyBook.Api.DTOs;

public record ProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    int TripsCount,
    int UpcomingCount
);

public record UpdateProfileRequest(string FullName, string? Phone);
