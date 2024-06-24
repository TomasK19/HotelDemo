using HotelDemo.DTO;

using MediatR;

namespace HotelDemo.Mediator.Queries.Hotels
{
    public class GetHotelQuery : IRequest<HotelDto>
    {
        public long HotelId { get; set; }
    }
}
