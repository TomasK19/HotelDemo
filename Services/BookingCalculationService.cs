using HotelDemo.Data;
using HotelDemo.Models;

namespace HotelDemo.Services;

public class BookingCalculationService(HotelBookingContext context) : IBookingCalculationService
{
    public async Task<decimal> CalculateTotalCostAsync(Booking booking)
    {
        var room = await context.Rooms.FindAsync(booking.RoomId);
        if (room == null)
            throw new ArgumentException("Invalid room ID");

        var roomRate = room.Price;
        var cleaningFee = 20m;
        var breakfastCost = booking.IncludeBreakfast
            ? 15m * booking.NumberOfGuests * booking.NumberOfNights
            : 0m;

        var totalCost = roomRate * booking.NumberOfNights + cleaningFee + breakfastCost;
        return totalCost;
    }
}
