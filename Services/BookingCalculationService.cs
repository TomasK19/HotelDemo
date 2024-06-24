using HotelDemo.Data;
using HotelDemo.Mediator.Queries.Bookings;
using HotelDemo.Models;

namespace HotelDemo.Services;

public class BookingCalculationService(HotelBookingContext context) : IBookingCalculationService
{
    public async Task<decimal> CalculateTotalCostAsync(CalculateBookingCostQuery query)
    {
        var room = await context.Rooms.FindAsync(query.RoomId);
        if (room == null)
            throw new ArgumentException("Invalid room ID");
        var numberOfNights = (int)(query.EndDate - query.StartDate).TotalDays;
        var roomRate = room.Price;
        var cleaningFee = 20m;
        var breakfastCost = query.IncludeBreakfast
            ? 15m * query.NumberOfGuests * numberOfNights
            : 0m;

        var totalCost = roomRate * numberOfNights + cleaningFee + breakfastCost;
        return totalCost;
    }
}
