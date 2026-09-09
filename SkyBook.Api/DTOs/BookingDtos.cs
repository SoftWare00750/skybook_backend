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
    decimal TotalPrice
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
    DateTime CreatedAt
);
