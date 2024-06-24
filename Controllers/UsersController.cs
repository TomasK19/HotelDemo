using HotelDemo.DTO;
using HotelDemo.Mediator.Commands.Email;
using HotelDemo.Mediator.Commands.Users;
using HotelDemo.Mediator.Queries.Users;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace HotelDemo.Controllers;

public class UsersController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        await Mediator.Send(command);
        return Ok("Registration successful. Please check your email for verification code.");

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command)
    {
        await Mediator.Send(command);
        return Ok("Email verified successfully.");
    }
}
