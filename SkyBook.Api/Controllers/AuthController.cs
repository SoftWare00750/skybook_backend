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
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, JwtTokenService jwt, ILogger<AuthController> logger)
    {
        _db = db;
        _jwt = jwt;
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
}
