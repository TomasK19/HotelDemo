using System;
using System.Threading.Tasks;
using FluentAssertions;
using HotelDemo.Data;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class BookingCalculationServiceTests
{
    private HotelBookingContext CreateContextWithRooms(params Room[] rooms)
    {
        var options = new DbContextOptionsBuilder<HotelBookingContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new HotelBookingContext(options);
        context.Rooms.AddRange(rooms);
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task CalculateTotalCostAsync_ShouldReturnCorrectCost_WithBreakfast()
    {
        // Arrange
        var room = new Room
        {
            Id = 1,
            Price = 100m,
            Type = "Standard"
        };

        var booking = new Booking
        {
            RoomId = room.Id,
            NumberOfNights = 2,
            NumberOfGuests = 2,
            IncludeBreakfast = true
        };

        var context = CreateContextWithRooms(room);
        var service = new BookingCalculationService(context);

        // Act
        var totalCost = await service.CalculateTotalCostAsync(booking);

        // Assert
        var expectedCost =
            (room.Price * booking.NumberOfNights)
            + 20m
            + (15m * booking.NumberOfGuests * booking.NumberOfNights);
        totalCost.Should().Be(expectedCost);
    }

    [Fact]
    public async Task CalculateTotalCostAsync_ShouldReturnCorrectCost_WithoutBreakfast()
    {
        // Arrange
        var room = new Room
        {
            Id = 2,
            Price = 150m,
            Type = "Deluxe"
        };

        var booking = new Booking
        {
            RoomId = room.Id,
            NumberOfNights = 3,
            NumberOfGuests = 1,
            IncludeBreakfast = false
        };

        var context = CreateContextWithRooms(room);
        var service = new BookingCalculationService(context);

        // Act
        var totalCost = await service.CalculateTotalCostAsync(booking);

        // Assert
        var expectedCost = (room.Price * booking.NumberOfNights) + 20m;
        totalCost.Should().Be(expectedCost);
    }

    [Fact]
    public async Task CalculateTotalCostAsync_ShouldThrowArgumentException_ForInvalidRoomId()
    {
        // Arrange
        var context = CreateContextWithRooms(); // No rooms added
        var service = new BookingCalculationService(context);

        var booking = new Booking
        {
            RoomId = 999, // Invalid room ID
            NumberOfNights = 1,
            NumberOfGuests = 1,
            IncludeBreakfast = false
        };

        // Act
        Func<Task> act = async () => await service.CalculateTotalCostAsync(booking);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid room ID");
    }
}
