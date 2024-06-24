using AutoMapper;

using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mediator.Queries.Hotels;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Mediator.Handlers.Hotels;

public class GetHotelsQueryHandler(HotelBookingContext context, IMapper mapper) : IRequestHandler<GetHotelsQuery, IEnumerable<HotelDto>>
{
    public async Task<IEnumerable<HotelDto>> Handle(GetHotelsQuery request, CancellationToken cancellationToken)
    {
        var hotels = await context
            .Hotels.Include(h => h.Rooms)
            .ThenInclude(r => r.Pictures)
            .ToListAsync();

        return mapper.Map<IEnumerable<HotelDto>>(hotels);
    }
}

