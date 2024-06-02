using HotelDemo.DTO;
using HotelDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await userService.RegisterUserAsync(userDto);
            return Ok("Registration successful. Please check your email for verification code.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await userService.AuthenticateUserAsync(userDto);
            var token = tokenService.GenerateJwtToken(user);
            return Ok(new { Token = token, UserId = user.Id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(EmailVerificationDto verificationDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await userService.VerifyEmailAsync(verificationDto);
            return Ok("Email verified successfully.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
