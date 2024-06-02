using System.ComponentModel.DataAnnotations;

namespace HotelDemo.DTO;

public class HotelDto
{
    public long Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Location { get; set; }

    public string PictureUrl { get; set; }

    [Range(6, 10)]
    public double Rating { get; set; }

    [Range(0, int.MaxValue)]
    public double NumberOfRatings { get; set; }

    [Range(1, 5)]
    public int StarCount { get; set; }

    public ICollection<RoomDto> Rooms { get; set; }
}
