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

    /// How this account was created / how it signs in: "password",
    /// "google", or "facebook". Purely informational (e.g. so the profile
    /// screen could show "Signed in with Google") — a social user can
    /// still exist even though PasswordHash is set to an unusable random
    /// value rather than left null, to avoid changing that column's
    /// nullability.
    public string Provider { get; set; } = "password";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
