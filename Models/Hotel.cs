namespace HotelDemo.Models;

public class Hotel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string PictureUrl { get; set; }
    public double Rating { get; set; }
    public int StarCount { get; set; }
    public double NumberOfRatings { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); // Initialize as empty list
    public ICollection<Room> Rooms { get; set; } = new List<Room>(); // Initia
}
