using AutoMapper;

using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mediator.Commands.Bookings;
using HotelDemo.Models;

using MediatR;
namespace HotelDemo.Mediator.Handlers;

public class CreateBookingCommandHandler(HotelBookingContext context, IMapper mapper) : IRequestHandler<CreateBookingCommand, BookingDto>
{
    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {

        if (request == null || request.HotelId <= 0 || request.RoomId <= 0)
        {
            throw new ArgumentException("Invalid booking details");
        }

        if (request.StartDate >= request.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        var hotel = await context.Hotels.FindAsync(request.HotelId);
        if (hotel == null)
            throw new ArgumentException(
                $"No hotel found with the provided HotelId: {request.HotelId}"
            );

        var room = await context.Rooms.FindAsync(request.RoomId);
        if (room == null)
            throw new ArgumentException(
                $"No room found with the provided RoomId: {request.RoomId}"
            );

        var booking = mapper.Map<Booking>(request);
        booking.Hotel = hotel;
        booking.Room = room;
        booking.UserId = request.UserId;
        booking.NumberOfNights = (int)(booking.EndDate - booking.StartDate).TotalDays;
        booking.TotalCost = booking.TotalCost;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return mapper.Map<BookingDto>(booking);
    }
}
