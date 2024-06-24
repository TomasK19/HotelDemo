using AutoMapper;
using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mediator.Queries.Bookings;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetUserBookingsQueryHandler(HotelBookingContext context, IMapper mapper) :
IRequestHandler<GetUserBookingsQuery, IEnumerable<BookingDto>>
{

    public async Task<IEnumerable<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await context
            .Bookings.Include(b => b.Hotel)
            .Include(b => b.Room)
            .Where(b => b.UserId == request.UserId)
            .ToListAsync();

        return mapper.Map<IEnumerable<BookingDto>>(bookings);

    }
}