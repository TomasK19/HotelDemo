using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Mediator.Queries.Users;
using HotelDemo.Models;
using HotelDemo.Services;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Mediator.Handlers.Users;

public class UserLoginQueryHandler(HotelBookingContext context, ITokenService tokenService) : IRequestHandler<UserLoginQuery, UserLoginResponseDTO>
{
    public async Task<UserLoginResponseDTO> Handle([FromQuery]UserLoginQuery request, CancellationToken cancellationToken)
    {
        var user = await AuthenticateUserAsync(request);
        var token = tokenService.GenerateJwtToken(user);
        var response = new UserLoginResponseDTO();

        response.UserId = user.Id;
        response.Token = token;
        return response;
    }

    public async Task<User> AuthenticateUserAsync(UserLoginQuery userDto)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Username == userDto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        if (!user.IsVerified)
            throw new UnauthorizedAccessException("User not verified");

        return user;
    }

}
