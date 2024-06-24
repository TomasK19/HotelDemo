using HotelDemo.Data;
using HotelDemo.Mediator.Commands.Users;
using HotelDemo.Models;
using HotelDemo.Services;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Mediator.Handlers.Users;

public class RegisterUserCommandHandler(HotelBookingContext context, IEmailService emailService) : IRequestHandler<RegisterUserCommand, Unit>
{
    public async Task<Unit> Handle([FromBody]RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (
            await context.Users.AnyAsync(u =>
                u.Email == command.Email || u.Username == command.Username
            )
        )
            throw new ArgumentException("Email or Username already exists");

        var user = new User
        {
            Email = command.Email,
            Username = command.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password),
            VerificationCode = GenerateVerificationCode(),
            IsVerified = false,
            RegistrationTimestamp = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);

        return Unit.Value;
    }

    private string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }
}

