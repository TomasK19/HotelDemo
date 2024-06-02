using System.ComponentModel.DataAnnotations;

namespace HotelDemo.DTO;

public class EmailVerificationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string VerificationCode { get; set; }
}
