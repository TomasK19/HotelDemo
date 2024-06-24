using HotelDemo.Mediator.Queries.Bookings;
using HotelDemo.Models;

namespace HotelDemo.Services;

public interface IBookingCalculationService
{
    Task<decimal> CalculateTotalCostAsync(CalculateBookingCostQuery query);
}
