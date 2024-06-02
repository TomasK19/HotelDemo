using System.ComponentModel.DataAnnotations;
using HotelDemo.Models;

namespace HotelDemo.DTO;

public class BookingDto
{
    public long Id { get; set; }

    [Required]
    public long HotelId { get; set; }

    public Hotel? Hotel { get; set; }

    [Required]
    public long RoomId { get; set; }

    public string? RoomType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public int NumberOfNights { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberOfGuests { get; set; }

    public bool IncludeBreakfast { get; set; }
    public decimal TotalCost { get; set; }
    public long UserId { get; set; }
}
