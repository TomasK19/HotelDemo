using AutoFixture;
using AutoMapper;
using FluentAssertions;
using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mappers;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelDemoTests;

public class HotelServiceTests
{
    private readonly HotelBookingContext _context;
    private readonly IFixture _fixture;
    private readonly HotelService _hotelService;
    private readonly IMapper _mapper;

    public HotelServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelBookingContext>()
            .UseInMemoryDatabase("HotelBookingTestDb")
            .Options;

        _context = new HotelBookingContext(options);
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _hotelService = new HotelService(_context, _mapper);
        _fixture = new Fixture();

        // Clear the database before each test
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Configure AutoFixture to handle circular references
        _fixture
            .Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetHotelsAsync_ReturnsMappedHotels()
    {
        // Arrange
        var hotels = _fixture
            .Build<Hotel>()
            .Without(h => h.Id) // Remove the default Id generation
            .CreateMany(5)
            .ToList();

        // Ensure unique IDs
        long id = 1;
        hotels.ForEach(h => h.Id = id++);

        await _context.Hotels.AddRangeAsync(hotels);
        await _context.SaveChangesAsync();

        // Act
        var result = await _hotelService.GetHotelsAsync();

        // Assert
        result.Should().BeEquivalentTo(_mapper.Map<IEnumerable<HotelDto>>(hotels));
    }

    [Fact]
    public async Task GetHotelByIdAsync_ValidId_ReturnsMappedHotel()
    {
        // Arrange
        var hotel = _fixture
            .Build<Hotel>()
            .Without(h => h.Id) // Remove the default Id generation
            .Create();
        hotel.Id = 1; // Assign a unique Id value

        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();

        // Act
        var result = await _hotelService.GetHotelByIdAsync(hotel.Id);

        // Assert
        result.Should().BeEquivalentTo(_mapper.Map<HotelDto>(hotel));
    }

    [Fact]
    public async Task GetHotelByIdAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = _fixture.Create<long>();

        // Act
        Func<Task> act = async () => await _hotelService.GetHotelByIdAsync(invalidId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid hotel ID");
    }
}
