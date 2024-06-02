using HotelDemo.Models;

namespace HotelDemo.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}
