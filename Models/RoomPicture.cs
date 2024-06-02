namespace HotelDemo.Models;

public class RoomPicture
{
    public int Id { get; set; }
    public string Url { get; set; }
    public long RoomId { get; set; }
    public Room Room { get; set; }
}
