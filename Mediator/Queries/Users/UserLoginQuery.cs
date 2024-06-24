using System.ComponentModel.DataAnnotations;

using HotelDemo.DTO;

using MediatR;

namespace HotelDemo.Mediator.Queries.Users;

public class UserLoginQuery : IRequest<UserLoginResponseDTO>
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}

