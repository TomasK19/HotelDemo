using AutoFixture;
using FluentAssertions;
using HotelDemo.Controllers;
using HotelDemo.DTO;
using HotelDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelDemoTests;

public class HotelsControllerTests
{
    private readonly HotelsController _controller;
    private readonly IFixture _fixture;
    private readonly Mock<IHotelService> _hotelServiceMock;

    public HotelsControllerTests()
    {
        _fixture = new Fixture();
        _hotelServiceMock = new Mock<IHotelService>();
        _controller = new HotelsController(_hotelServiceMock.Object);
    }

    [Fact]
    public async Task GetHotels_ReturnsOkWithHotels()
    {
        // Arrange
        var hotelDTOs = _fixture.CreateMany<HotelDto>(5).ToList();
        _hotelServiceMock.Setup(s => s.GetHotelsAsync()).ReturnsAsync(hotelDTOs);

        // Act
        var result = await _controller.GetHotels();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeEquivalentTo(hotelDTOs);
    }

    [Fact]
    public async Task GetHotel_ValidId_ReturnsOkWithHotel()
    {
        // Arrange
        var hotelDTO = _fixture.Create<HotelDto>();
        _hotelServiceMock.Setup(s => s.GetHotelByIdAsync(hotelDTO.Id)).ReturnsAsync(hotelDTO);

        // Act
        var result = await _controller.GetHotel(hotelDTO.Id);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeEquivalentTo(hotelDTO);
    }

    [Fact]
    public async Task GetHotel_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = _fixture.Create<long>();
        _hotelServiceMock
            .Setup(s => s.GetHotelByIdAsync(invalidId))
            .ThrowsAsync(new ArgumentException("Invalid hotel ID"));

        // Act
        var result = await _controller.GetHotel(invalidId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult.Value.Should().BeEquivalentTo(new { error = "Invalid hotel ID" });
    }
}
