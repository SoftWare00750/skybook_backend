namespace SkyBook.Api.DTOs;

public record CreateBookingRequest(
    string Airline,
    string FlightCode,
    string DepartCode,
    string ArriveCode,
    string DepartTime,
    string ArriveTime,
    DateOnly DepartDate,
    string Duration,
    string Stops,
    string SeatNumber,
    string CabinClass,
    int Passengers,
    decimal TotalPrice,
    // Set from the POST /api/payments/simulate response that should have
    // just run on the client before this call. PaymentMethod of "wallet"
    // is the only one that debits the wallet ledger here — card / bank
    // transfer / other are charged directly via the simulated payment and
    // just get recorded on the booking for the receipt.
    string? PaymentMethod,
    string? PaymentMethodLabel,
    string? PaymentReference
);

public record BookingResponse(
    Guid Id,
    string BookingRef,
    string Airline,
    string FlightCode,
    string DepartCode,
    string ArriveCode,
    string DepartTime,
    string ArriveTime,
    DateOnly DepartDate,
    string Duration,
    string Stops,
    string SeatNumber,
    string CabinClass,
    int Passengers,
    decimal TotalPrice,
    string Status,
    bool IsUpcoming,
    string? PaymentMethod,
    string? PaymentMethodLabel,
    string? PaymentReference,
    DateTime CreatedAt
);
