using HotelDemo.DTO;
using HotelDemo.Models;

namespace HotelDemo.Services;

public interface IUserService
{
    Task RegisterUserAsync(UserRegistrationDto userDto);
    Task<User> AuthenticateUserAsync(UserLoginDto userDto);
    Task VerifyEmailAsync(EmailVerificationDto verificationDto);
}
