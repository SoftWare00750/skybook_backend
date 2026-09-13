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

    /// How this booking was paid for: "card" | "bank_transfer" | "wallet" |
    /// "other". Set from the result of POST /api/payments/simulate.
    public string? PaymentMethod { get; set; }

    /// Display label of the method used, e.g. "Visa •••• 4242", and the
    /// simulated payment's reference code, e.g. "PAY-7F3K9QZL". Both are
    /// shown on the booking confirmation / details screens.
    public string? PaymentMethodLabel { get; set; }
    public string? PaymentReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
