using AutoMapper;
using HotelDemo.Data;
using HotelDemo.DTO;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Services;

public class HotelService(HotelBookingContext context, IMapper mapper) : IHotelService
{
    public async Task<IEnumerable<HotelDto>> GetHotelsAsync()
    {
        var hotels = await context
            .Hotels.Include(h => h.Rooms)
            .ThenInclude(r => r.Pictures)
            .ToListAsync();

        return mapper.Map<IEnumerable<HotelDto>>(hotels);
    }

    public async Task<HotelDto> GetHotelByIdAsync(long id)
    {
        var hotel = await context
            .Hotels.Include(h => h.Rooms)
            .ThenInclude(r => r.Pictures)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hotel == null)
            throw new ArgumentException("Invalid hotel ID");

        return mapper.Map<HotelDto>(hotel);
    }
}
