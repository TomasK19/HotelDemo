using System.ComponentModel.DataAnnotations;

namespace HotelDemo.DTO;

public class UserLoginResponseDTO
{
    [Required]
    public long UserId { get; set; }

    [Required]

    public string Token { get; set; }

}
