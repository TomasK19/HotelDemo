namespace HotelDemo.Models;

public class Room
{
    public long Id { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; }
    public long HotelId { get; set; }
    public Hotel Hotel { get; set; }
    public int MaxNumberOfGuests { get; set; }
    public ICollection<RoomPicture> Pictures { get; set; } = new List<RoomPicture>();
}
