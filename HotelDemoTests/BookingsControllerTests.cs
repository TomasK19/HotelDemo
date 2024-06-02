using System.Security.Claims;
using AutoMapper;
using HotelDemo.Controllers;
using HotelDemo.DTO;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelDemoTests
{
    public class BookingsControllerTests
    {
        private readonly Mock<IBookingCalculationService> _bookingCalculationServiceMock;
        private readonly Mock<IBookingService> _bookingServiceMock;
        private readonly BookingsController _controller;
        private readonly Mock<IMapper> _mapperMock;

        public BookingsControllerTests()
        {
            _bookingCalculationServiceMock = new Mock<IBookingCalculationService>();
            _bookingServiceMock = new Mock<IBookingService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new BookingsController(
                _bookingCalculationServiceMock.Object,
                _bookingServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task CalculateTotalCost_ReturnsOkResult_WithTotalCost()
        {
            // Arrange
            var bookingDto = new BookingCostCalculationDto();
            var booking = new Booking();
            var expectedTotalCost = 100m;

            _mapperMock.Setup(m => m.Map<Booking>(bookingDto)).Returns(booking);
            _bookingCalculationServiceMock
                .Setup(s => s.CalculateTotalCostAsync(booking))
                .ReturnsAsync(expectedTotalCost);

            // Act
            var result = await _controller.CalculateTotalCost(bookingDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedTotalCost, okResult.Value);
        }

        [Fact]
        public async Task CalculateTotalCost_ReturnsBadRequest_ForInvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid model state");

            // Act
            var result = await _controller.CalculateTotalCost(new BookingCostCalculationDto());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var serializableError = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.Equal(
                _controller.ModelState["Error"].Errors[0].ErrorMessage,
                ((string[])serializableError["Error"])[0]
            );
        }

        [Fact]
        public async Task CreateBooking_ReturnsCreatedAtActionResult_WithBookingDto()
        {
            // Arrange
            var bookingDto = new BookingDto { HotelId = 1, RoomId = 1 };
            var createdBookingDto = new BookingDto
            {
                Id = 1,
                HotelId = 1,
                RoomId = 1
            };
            var userId = "1";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new Claim[] { new("id", userId) }, "mock")
                    )
                }
            };

            _bookingServiceMock
                .Setup(s => s.CreateBookingAsync(bookingDto, long.Parse(userId)))
                .ReturnsAsync(createdBookingDto);

            // Act
            var result = await _controller.CreateBooking(bookingDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(createdBookingDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateBooking_ReturnsBadRequest_ForInvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid model state");

            // Act
            var result = await _controller.CreateBooking(new BookingDto());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var serializableError = Assert.IsType<SerializableError>(badRequestResult.Value);

            foreach (var key in _controller.ModelState.Keys)
            {
                var modelStateErrors = _controller
                    .ModelState[key]
                    .Errors.Select(e => e.ErrorMessage)
                    .ToArray();
                var serializableErrorMessages = (string[])serializableError[key];
                Assert.Equal(modelStateErrors, serializableErrorMessages);
            }
        }

        [Fact]
        public async Task CreateBooking_ReturnsUnauthorized_ForMissingUserId()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await _controller.CreateBooking(new BookingDto());

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_ReturnsBadRequest_ForArgumentException()
        {
            // Arrange
            var bookingDto = new BookingDto { HotelId = 1, RoomId = 1 };
            var userId = "1";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new Claim[] { new("id", userId) }, "mock")
                    )
                }
            };

            _bookingServiceMock
                .Setup(s => s.CreateBookingAsync(bookingDto, long.Parse(userId)))
                .ThrowsAsync(new ArgumentException("Invalid booking details"));

            // Act
            var result = await _controller.CreateBooking(bookingDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid booking details", badRequestResult.Value);
        }

        [Fact]
        public async Task GetBooking_ReturnsOkResult_WithBookingDto()
        {
            // Arrange
            var bookingId = 1;
            var bookingDto = new BookingDto
            {
                Id = bookingId,
                HotelId = 1,
                RoomId = 1
            };

            _bookingServiceMock
                .Setup(s => s.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(bookingDto);

            // Act
            var result = await _controller.GetBooking(bookingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(bookingDto, okResult.Value);
        }

        [Fact]
        public async Task GetBooking_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            var bookingId = 999;

            _bookingServiceMock
                .Setup(s => s.GetBookingByIdAsync(bookingId))
                .ThrowsAsync(new ArgumentException("Invalid booking ID"));

            // Act
            var result = await _controller.GetBooking(bookingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Invalid booking ID", notFoundResult.Value);
        }

        [Fact]
        public async Task GetUserBookings_ReturnsOkResult_WithBookings()
        {
            // Arrange
            var userId = "1";
            var bookings = new List<BookingDto>
            {
                new()
                {
                    Id = 1,
                    HotelId = 1,
                    RoomId = 1
                }
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new Claim[] { new("id", userId) }, "mock")
                    )
                }
            };

            _bookingServiceMock
                .Setup(s => s.GetUserBookingsAsync(long.Parse(userId)))
                .ReturnsAsync(bookings);

            // Act
            var result = await _controller.GetUserBookings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(bookings, okResult.Value);
        }

        [Fact]
        public async Task GetUserBookings_ReturnsUnauthorized_ForMissingUserId()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await _controller.GetUserBookings();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result.Result);
        }
    }
}
