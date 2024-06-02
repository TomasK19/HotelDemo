using System.ComponentModel.DataAnnotations;

namespace HotelDemo.DTO;

public class BookingCostCalculationDto
{
    [Required]
    public long RoomId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int NumberOfGuests { get; set; }

    [Required]
    public bool IncludeBreakfast { get; set; }
}
