using AutoMapper;
using HotelDemo.DTO;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController(
    IBookingCalculationService bookingCalculationService,
    IBookingService bookingService,
    IMapper mapper
) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<decimal>> CalculateTotalCost(
        [FromBody] BookingCostCalculationDto bookingDto
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var totalCost = await bookingCalculationService.CalculateTotalCostAsync(
            mapper.Map<Booking>(bookingDto)
        );
        return Ok(totalCost);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] BookingDto bookingDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var booking = await bookingService.CreateBookingAsync(bookingDto, long.Parse(userId));
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetBooking(long id)
    {
        try
        {
            var booking = await bookingService.GetBookingByIdAsync(id);
            return Ok(booking);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("user-bookings")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetUserBookings()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var bookings = await bookingService.GetUserBookingsAsync(long.Parse(userId));
        return Ok(bookings);
    }
}
