using HotelDemo.DTO;
using HotelDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelDemo.Controllers;

[ApiController]
[Route("api/hotels")]
public class HotelsController(IHotelService hotelService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
    {
        var hotels = await hotelService.GetHotelsAsync();
        return Ok(hotels);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HotelDto>> GetHotel(long id)
    {
        try
        {
            var hotel = await hotelService.GetHotelByIdAsync(id);
            return Ok(hotel);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
