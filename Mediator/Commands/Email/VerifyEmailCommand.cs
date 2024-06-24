using System.ComponentModel.DataAnnotations;

using MediatR;

namespace HotelDemo.Mediator.Commands.Email;

public class VerifyEmailCommand : IRequest<Unit>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string VerificationCode { get; set; }
}
