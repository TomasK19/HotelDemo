using AutoMapper;
using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Services;

public class BookingService(HotelBookingContext context, IMapper mapper) : IBookingService
{
    public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(long userId)
    {
        var bookings = await context
            .Bookings.Include(b => b.Hotel)
            .Include(b => b.Room)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        return mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto> GetBookingByIdAsync(long id)
    {
        var booking = await context
            .Bookings.Include(b => b.Hotel)
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            throw new ArgumentException("Invalid booking ID");

        return mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(BookingDto bookingDto, long userId)
    {
        if (bookingDto == null || bookingDto.HotelId <= 0 || bookingDto.RoomId <= 0)
        {
            throw new ArgumentException("Invalid booking details");
        }

        if (bookingDto.StartDate >= bookingDto.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        var hotel = await context.Hotels.FindAsync(bookingDto.HotelId);
        if (hotel == null)
            throw new ArgumentException(
                $"No hotel found with the provided HotelId: {bookingDto.HotelId}"
            );

        var room = await context.Rooms.FindAsync(bookingDto.RoomId);
        if (room == null)
            throw new ArgumentException(
                $"No room found with the provided RoomId: {bookingDto.RoomId}"
            );

        var booking = mapper.Map<Booking>(bookingDto);
        booking.Hotel = hotel;
        booking.Room = room;
        booking.UserId = userId;
        booking.NumberOfNights = (int)(booking.EndDate - booking.StartDate).TotalDays;
        booking.TotalCost = booking.TotalCost;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return mapper.Map<BookingDto>(booking);
    }
}
