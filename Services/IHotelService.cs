using HotelDemo.DTO;

namespace HotelDemo.Services;

public interface IHotelService
{
    Task<IEnumerable<HotelDto>> GetHotelsAsync();
    Task<HotelDto> GetHotelByIdAsync(long id);
}
