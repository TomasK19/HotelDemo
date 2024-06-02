using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Services;

public class UserService(HotelBookingContext context, IEmailService emailService) : IUserService
{
    public async Task RegisterUserAsync(UserRegistrationDto userDto)
    {
        if (
            await context.Users.AnyAsync(u =>
                u.Email == userDto.Email || u.Username == userDto.Username
            )
        )
            throw new ArgumentException("Email or Username already exists");

        var user = new User
        {
            Email = userDto.Email,
            Username = userDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
            VerificationCode = GenerateVerificationCode(),
            IsVerified = false,
            RegistrationTimestamp = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await emailService.SendVerificationEmailAsync(user.Email, user.VerificationCode);
    }

    public async Task<User> AuthenticateUserAsync(UserLoginDto userDto)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Username == userDto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        if (!user.IsVerified)
            throw new UnauthorizedAccessException("User not verified");

        return user;
    }

    public async Task VerifyEmailAsync(EmailVerificationDto verificationDto)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == verificationDto.Email);
        if (user == null || user.VerificationCode != verificationDto.VerificationCode)
            throw new UnauthorizedAccessException("Invalid verification code");

        user.IsVerified = true;
        user.VerificationCode = null;

        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    private string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }
}
