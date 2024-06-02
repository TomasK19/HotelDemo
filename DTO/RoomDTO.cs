namespace HotelDemo.DTO;

public class RoomDto
{
    public long Id { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; }
    public int MaxNumberOfGuests { get; set; }

    public ICollection<RoomPictureDto> Pictures { get; set; }
}
