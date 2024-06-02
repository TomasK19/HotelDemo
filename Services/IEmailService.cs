namespace HotelDemo.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string verificationCode);
}
