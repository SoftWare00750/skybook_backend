namespace SkyBook.Api.Models;

/// <summary>
/// A single flight booking made by a user. Mirrors the "bookings" table in
/// Supabase. Whether a booking counts as "Upcoming" or "Past" is derived
/// from <see cref="DepartDate"/> vs. now rather than stored, so it can
/// never drift out of sync.
/// </summary>
public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    /// Short human-friendly reference shown to the user, e.g. "ABC12345".
    public string BookingRef { get; set; } = string.Empty;

    public string Airline { get; set; } = string.Empty;
    public string FlightCode { get; set; } = string.Empty;

    public string DepartCode { get; set; } = string.Empty;
    public string ArriveCode { get; set; } = string.Empty;
    public string DepartTime { get; set; } = string.Empty; // e.g. "08:30"
    public string ArriveTime { get; set; } = string.Empty; // e.g. "20:15"
    public DateOnly DepartDate { get; set; }

    public string Duration { get; set; } = string.Empty; // e.g. "7h 45m"
    public string Stops { get; set; } = "Non-stop";

    public string SeatNumber { get; set; } = string.Empty;
    public string CabinClass { get; set; } = "Economy";
    public int Passengers { get; set; } = 1;

    public decimal TotalPrice { get; set; }

    /// "Confirmed" or "Cancelled". Upcoming/Past is derived from DepartDate.
    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
