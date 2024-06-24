using HotelDemo.DTO;
using MediatR;

namespace HotelDemo.Mediator.Queries.Bookings;
public class GetUserBookingsQuery : IRequest<IEnumerable<BookingDto>>
{
    public long UserId { get; set; }
}