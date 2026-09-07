namespace SkyBook.Api.Models;

/// <summary>
/// Mirrors the "users" table in Supabase (Postgres). Only the columns
/// needed for account creation and sign-in — this backend intentionally
/// stays scoped to auth/user info, matching the SkyBook app's needs.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
