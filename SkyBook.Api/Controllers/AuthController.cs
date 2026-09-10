using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Data;
using SkyBook.Api.DTOs;
using SkyBook.Api.Models;
using SkyBook.Api.Services;

namespace SkyBook.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly SocialAuthService _socialAuth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, JwtTokenService jwt, SocialAuthService socialAuth, ILogger<AuthController> logger)
    {
        _db = db;
        _jwt = jwt;
        _socialAuth = socialAuth;
        _logger = logger;
    }

    // POST /api/auth/signup
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ErrorResponse("Full name, email, and password are required."));
        }

        if (request.Password.Length < 8)
        {
            return BadRequest(new ErrorResponse("Password must be at least 8 characters."));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (exists)
        {
            return Conflict(new ErrorResponse("An account with this email already exists."));
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        _logger.LogInformation("New user signed up: {Email}", user.Email);

        return Created(string.Empty, new AuthResponse(token, user.FullName, user.Email));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrPhone) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ErrorResponse("Email/phone and password are required."));
        }

        var identifier = request.EmailOrPhone.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == identifier || u.Phone == identifier);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ErrorResponse("Invalid email/phone or password."));
        }

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.FullName, user.Email));
    }

    // POST /api/auth/google
    // Body: { "idToken": "<Google ID token from the Flutter app>" }
    // Verifies the token server-side, then finds or creates a matching
    // user purely by email and signs them in — same as a normal login,
    // just with Google standing in for a password.
    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        try
        {
            var (email, name) = await _socialAuth.VerifyGoogleIdTokenAsync(request.IdToken);
            var user = await FindOrCreateSocialUserAsync(email, name, provider: "google");
            var token = _jwt.GenerateToken(user);
            return Ok(new AuthResponse(token, user.FullName, user.Email));
        }
        catch (SocialAuthException ex)
        {
            return Unauthorized(new ErrorResponse(ex.Message));
        }
    }

    // POST /api/auth/facebook
    // Body: { "accessToken": "<Facebook access token from the Flutter app>" }
    [HttpPost("facebook")]
    public async Task<IActionResult> FacebookAuth([FromBody] FacebookAuthRequest request)
    {
        try
        {
            var (email, name) = await _socialAuth.VerifyFacebookAccessTokenAsync(request.AccessToken);
            var user = await FindOrCreateSocialUserAsync(email, name, provider: "facebook");
            var token = _jwt.GenerateToken(user);
            return Ok(new AuthResponse(token, user.FullName, user.Email));
        }
        catch (SocialAuthException ex)
        {
            return Unauthorized(new ErrorResponse(ex.Message));
        }
    }

    // Shared by both social providers: sign in if an account with this
    // email already exists (regardless of how it was originally created —
    // so someone who signed up with a password can still use "Continue
    // with Google" later using the same email), otherwise create one.
    // Social accounts get a random, unusable password hash rather than a
    // null one, so the "password_hash not null" column doesn't need to
    // change for them.
    private async Task<User> FindOrCreateSocialUserAsync(string email, string name, string provider)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (existing != null)
        {
            return existing;
        }

        var user = new User
        {
            FullName = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
            Provider = provider,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("New user signed up via {Provider}: {Email}", provider, user.Email);
        return user;
    }
}
