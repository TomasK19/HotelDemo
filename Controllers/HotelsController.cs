using HotelDemo.DTO;
using HotelDemo.Mediator.Queries.Hotels;
using HotelDemo.Services;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace HotelDemo.Controllers;

public class HotelsController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("all")]
    public async Task<ActionResult> GetHotels()
    {
        var result = await Mediator.Send(new GetHotelsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetHotel([FromQuery] GetHotelQuery query, long id)
    {
            query.HotelId = id;
            var result = await Mediator.Send(query);
            return Ok(result);
    }
}
