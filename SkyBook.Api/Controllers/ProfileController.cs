using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Data;
using SkyBook.Api.DTOs;
using SkyBook.Api.Services;

namespace SkyBook.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = this.GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new ErrorResponse("User not found."));

        var bookings = await _db.Bookings.Where(b => b.UserId == userId).ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return Ok(new ProfileResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.CreatedAt,
            bookings.Count,
            bookings.Count(b => b.DepartDate >= today)
        ));
    }

    // PUT /api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new ErrorResponse("Full name is required."));
        }

        var userId = this.GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new ErrorResponse("User not found."));

        user.FullName = request.FullName.Trim();
        user.Phone = request.Phone?.Trim();
        await _db.SaveChangesAsync();

        var bookings = await _db.Bookings.Where(b => b.UserId == userId).ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return Ok(new ProfileResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.CreatedAt,
            bookings.Count,
            bookings.Count(b => b.DepartDate >= today)
        ));
    }
}
