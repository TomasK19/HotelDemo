using HotelDemo.DTO;

namespace HotelDemo.Services;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetUserBookingsAsync(long userId);
    Task<BookingDto> GetBookingByIdAsync(long id);
    Task<BookingDto> CreateBookingAsync(BookingDto bookingDto, long userId);
}
