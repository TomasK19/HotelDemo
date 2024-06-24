using HotelDemo.Mediator.Queries.Bookings;
using HotelDemo.Services;

using MediatR;

namespace HotelDemo.Mediator.Handlers.Bookings;

public class CalculateBookingCostQueryHandler(IBookingCalculationService bookingCalculationService) : IRequestHandler<CalculateBookingCostQuery, decimal>
{
    public async Task<decimal> Handle(CalculateBookingCostQuery request, CancellationToken cancellationToken)
    {
        var result = await bookingCalculationService.CalculateTotalCostAsync(request);
        return result;
    }
}
