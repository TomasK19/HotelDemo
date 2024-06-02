namespace HotelDemo.Models;

public class Booking
{
    public long Id { get; set; }
    public long HotelId { get; set; }
    public Hotel Hotel { get; set; }
    public long RoomId { get; set; }
    public Room Room { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfNights { get; set; }
    public int NumberOfGuests { get; set; }
    public bool IncludeBreakfast { get; set; }
    public decimal TotalCost { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
}
