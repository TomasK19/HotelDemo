using AutoMapper;

using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mediator.Queries.Hotels;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Mediator.Handlers.Hotels;

public class GetHotelQueryHandler(HotelBookingContext context, IMapper mapper) : IRequestHandler<GetHotelQuery, HotelDto>
{
    public async Task<HotelDto> Handle(GetHotelQuery request, CancellationToken cancellationToken)
    {
        var hotel = await context
            .Hotels.Include(h => h.Rooms)
            .ThenInclude(r => r.Pictures)
            .FirstOrDefaultAsync(h => h.Id == request.HotelId);

        if (hotel == null)
            throw new ArgumentException("Invalid hotel ID");

        return mapper.Map<HotelDto>(hotel);
    }
}

