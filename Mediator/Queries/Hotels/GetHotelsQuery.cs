using HotelDemo.DTO;

using MediatR;

namespace HotelDemo.Mediator.Queries.Hotels
{
    public class GetHotelsQuery : IRequest<IEnumerable<HotelDto>>
    {
    }
}
