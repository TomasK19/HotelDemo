using Microsoft.AspNetCore.Mvc;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using HotelDemo.Mediator.Queries.Bookings;
using HotelDemo.Mediator.Commands.Bookings;

namespace HotelDemo.Controllers;

public class BookingsController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateTotalCost([FromBody] CalculateBookingCostQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<ActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        command.UserId = long.Parse(userId);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("user-bookings")]
    [Authorize]
    public async Task<ActionResult> GetUserBookings([FromQuery] GetUserBookingsQuery query)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        query.UserId = long.Parse(userId);

        var result = await Mediator.Send(query);
        return Ok(result);
    }

}
