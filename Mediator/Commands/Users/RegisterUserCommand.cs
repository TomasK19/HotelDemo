using System.ComponentModel.DataAnnotations;

using MediatR;


namespace HotelDemo.Mediator.Commands.Users;

public class RegisterUserCommand : IRequest<Unit>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}
