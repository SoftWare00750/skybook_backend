namespace SkyBook.Api.DTOs;

public record SignupRequest(string FullName, string Email, string? Phone, string Password);

public record LoginRequest(string EmailOrPhone, string Password);

/// idToken from Google Sign-In on the Flutter app (GoogleSignInAuthentication.idToken).
/// Verified server-side against Google before we trust anything in it.
public record GoogleAuthRequest(string IdToken);

/// accessToken from the Facebook Login SDK on the Flutter app
/// (LoginResult.accessToken.tokenString). Verified server-side against
/// the Facebook Graph API before we trust anything derived from it.
public record FacebookAuthRequest(string AccessToken);

public record AuthResponse(string Token, string FullName, string Email);

public record ErrorResponse(string Message);
