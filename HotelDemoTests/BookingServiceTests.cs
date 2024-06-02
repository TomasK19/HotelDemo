using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mappers;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HotelDemoTests
{
    public class BookingServiceTests
    {
        private readonly IFixture _fixture;
        private readonly IMapper _mapper;

        public BookingServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();
        }

        private DbContextOptions<HotelBookingContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<HotelBookingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetUserBookingsAsync_ShouldReturnBookings_ForGivenUserId()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = _fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);
            var userId = 1;

            var hotel = _fixture.Create<Hotel>();
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var room = _fixture.Build<Room>().With(r => r.Hotel, hotel).Create();
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var bookings = new List<Booking>
            {
                new()
                {
                    UserId = userId,
                    Hotel = hotel,
                    Room = room,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    NumberOfNights = 1,
                    NumberOfGuests = 2,
                    IncludeBreakfast = true,
                    TotalCost = 100
                },
                new()
                {
                    UserId = userId,
                    Hotel = hotel,
                    Room = room,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    NumberOfNights = 1,
                    NumberOfGuests = 2,
                    IncludeBreakfast = true,
                    TotalCost = 100
                },
                new()
                {
                    UserId = userId,
                    Hotel = hotel,
                    Room = room,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    NumberOfNights = 1,
                    NumberOfGuests = 2,
                    IncludeBreakfast = true,
                    TotalCost = 100
                }
            };

            context.Bookings.AddRange(bookings);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetUserBookingsAsync(userId);

            // Assert
            result.Should().HaveCount(3);
            result.All(b => b.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetBookingByIdAsync_ShouldReturnBooking_ForValidId()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = _fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);

            var hotel = _fixture.Create<Hotel>();
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var room = _fixture.Build<Room>().With(r => r.Hotel, hotel).Create();
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                Hotel = hotel,
                Room = room,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                NumberOfNights = 1,
                NumberOfGuests = 2,
                IncludeBreakfast = true,
                TotalCost = 100
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetBookingByIdAsync(booking.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(booking.Id);
        }

        [Fact]
        public async Task GetBookingByIdAsync_ShouldThrowException_ForInvalidId()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = _fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);
            var invalidId = -1;

            // Act
            Func<Task> act = async () => await service.GetBookingByIdAsync(invalidId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid booking ID");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldCreateBooking_ForValidInput()
        {
            // Arrange
            var fixture = new Fixture();
            fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);

            var bookingDto = fixture
                .Build<BookingDto>()
                .Without(b => b.Hotel)
                .Without(b => b.RoomType)
                .With(b => b.StartDate, DateTime.Now.AddDays(1)) // Ensure valid start date
                .With(b => b.EndDate, DateTime.Now.AddDays(2)) // Ensure valid end date
                .With(b => b.TotalCost, 100M)
                .Create();

            var userId = 1;

            var hotel = fixture.Build<Hotel>().With(h => h.Id, bookingDto.HotelId).Create();
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var room = fixture
                .Build<Room>()
                .With(r => r.Id, bookingDto.RoomId)
                .With(r => r.Hotel, hotel)
                .Create();
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            // Act
            var result = await service.CreateBookingAsync(bookingDto, userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.TotalCost.Should().Be(100M);

            var bookingInDb = await context.Bookings.FirstOrDefaultAsync(b => b.Id == result.Id);
            bookingInDb.Should().NotBeNull();
            bookingInDb.TotalCost.Should().Be(100M);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_ForInvalidHotelOrRoom()
        {
            // Arrange
            var fixture = new Fixture();
            fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);
            var bookingDto = fixture
                .Build<BookingDto>()
                .Without(b => b.Hotel)
                .Without(b => b.RoomType)
                .With(b => b.StartDate, DateTime.Now.AddDays(1)) // Ensure valid start date
                .With(b => b.EndDate, DateTime.Now.AddDays(2)) // Ensure valid end date
                .With(b => b.HotelId, 999) // Ensure hotel does not exist
                .With(b => b.RoomId, 999) // Ensure room does not exist
                .Create();
            var userId = 1;

            // Act
            Func<Task> act = async () => await service.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("No hotel found with the provided HotelId: 999");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_ForStartDateAfterEndDate()
        {
            // Arrange
            var fixture = new Fixture();
            fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);

            var bookingDto = fixture
                .Build<BookingDto>()
                .With(b => b.StartDate, DateTime.Now.AddDays(2))
                .With(b => b.EndDate, DateTime.Now.AddDays(1))
                .With(b => b.HotelId, 1) // Ensure hotel does not exist
                .With(b => b.RoomId, 1) // Ensure room does not exist
                .Create();
            var userId = 1;

            // Act
            Func<Task> act = async () => await service.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Start date must be before end date");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_ForNullValuesInDTO()
        {
            // Arrange
            var fixture = new Fixture();
            fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);

            var bookingDto = new BookingDto(); // Creating an empty DTO with null values
            var userId = 1;

            // Act
            Func<Task> act = async () => await service.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Invalid booking details");
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_ForInvalidRoom()
        {
            // Arrange
            var fixture = new Fixture();
            fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = CreateNewContextOptions();
            using var context = new HotelBookingContext(options);
            var calculationServiceMock = fixture.Freeze<Mock<IBookingCalculationService>>();
            var service = new BookingService(context, _mapper);

            var validHotel = fixture.Build<Hotel>().Create();
            context.Hotels.Add(validHotel);
            await context.SaveChangesAsync();

            var bookingDto = fixture
                .Build<BookingDto>()
                .Without(b => b.Hotel)
                .Without(b => b.RoomType)
                .With(b => b.StartDate, DateTime.Now.AddDays(1)) // Ensure valid start date
                .With(b => b.EndDate, DateTime.Now.AddDays(2)) // Ensure valid end date
                .With(b => b.HotelId, validHotel.Id) // Use valid hotel
                .With(b => b.RoomId, 999) // Ensure room does not exist
                .Create();
            var userId = 1;

            // Act
            Func<Task> act = async () => await service.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("No room found with the provided RoomId: 999");
        }
    }
}
