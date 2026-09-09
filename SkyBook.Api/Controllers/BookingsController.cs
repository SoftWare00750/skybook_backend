using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Api.Data;
using SkyBook.Api.DTOs;
using SkyBook.Api.Models;
using SkyBook.Api.Services;

namespace SkyBook.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(AppDbContext db, ILogger<BookingsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/bookings
    // Returns every booking for the signed-in user, newest first. The
    // Flutter app splits these into "Upcoming" / "Past" tabs itself using
    // the IsUpcoming flag, so no separate endpoint is needed for that.
    [HttpGet]
    public async Task<IActionResult> GetBookings()
    {
        var userId = this.GetUserId();
        var bookings = await _db.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.DepartDate)
            .ToListAsync();

        return Ok(bookings.Select(ToResponse));
    }

    // GET /api/bookings/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var userId = this.GetUserId();
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (booking == null) return NotFound(new ErrorResponse("Booking not found."));
        return Ok(ToResponse(booking));
    }

    // POST /api/bookings
    // Called once payment succeeds on the Flutter app, so a real record
    // exists for "My Bookings" / "My Trips" / the wallet debit.
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Airline) || string.IsNullOrWhiteSpace(request.DepartCode) ||
            string.IsNullOrWhiteSpace(request.ArriveCode))
        {
            return BadRequest(new ErrorResponse("Airline, departure and arrival airports are required."));
        }

        if (request.TotalPrice < 0)
        {
            return BadRequest(new ErrorResponse("Total price cannot be negative."));
        }

        var userId = this.GetUserId();

        var booking = new Booking
        {
            UserId = userId,
            BookingRef = GenerateBookingRef(),
            Airline = request.Airline,
            FlightCode = request.FlightCode,
            DepartCode = request.DepartCode.ToUpperInvariant(),
            ArriveCode = request.ArriveCode.ToUpperInvariant(),
            DepartTime = request.DepartTime,
            ArriveTime = request.ArriveTime,
            DepartDate = request.DepartDate,
            Duration = request.Duration,
            Stops = request.Stops,
            SeatNumber = request.SeatNumber,
            CabinClass = request.CabinClass,
            Passengers = request.Passengers <= 0 ? 1 : request.Passengers,
            TotalPrice = request.TotalPrice,
            Status = "Confirmed",
        };

        _db.Bookings.Add(booking);

        // Debit the wallet in the same transaction so a booking always has
        // a matching ledger entry.
        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId,
            Label = $"Flight Booking · {booking.Airline} {booking.FlightCode}",
            Amount = -booking.TotalPrice,
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Booking {Ref} created for user {UserId}", booking.BookingRef, userId);

        return Created(string.Empty, ToResponse(booking));
    }

    private static string GenerateBookingRef()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = Random.Shared;
        return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    private static BookingResponse ToResponse(Booking b) => new(
        b.Id,
        b.BookingRef,
        b.Airline,
        b.FlightCode,
        b.DepartCode,
        b.ArriveCode,
        b.DepartTime,
        b.ArriveTime,
        b.DepartDate,
        b.Duration,
        b.Stops,
        b.SeatNumber,
        b.CabinClass,
        b.Passengers,
        b.TotalPrice,
        b.Status,
        b.DepartDate >= DateOnly.FromDateTime(DateTime.UtcNow),
        b.CreatedAt
    );
}
