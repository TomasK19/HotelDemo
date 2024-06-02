using HotelDemo.Models;

namespace HotelDemo.Services;

public interface IBookingCalculationService
{
    Task<decimal> CalculateTotalCostAsync(Booking booking);
}
