namespace HotelDemo.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string VerificationCode { get; set; }
    public bool IsVerified { get; set; }
    public DateTime RegistrationTimestamp { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
