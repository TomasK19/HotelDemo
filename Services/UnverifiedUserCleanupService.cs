using HotelDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelDemo.Services;

public class UnverifiedUserCleanupService(HotelBookingContext context)
{
    private readonly TimeSpan _verificationExpiryTime = TimeSpan.FromMinutes(10);

    public async Task CleanupUnverifiedUsersAsync()
    {
        var expiryDate = DateTime.UtcNow - _verificationExpiryTime;
        var unverifiedUsers = await context
            .Users.Where(u => !u.IsVerified && u.RegistrationTimestamp < expiryDate)
            .ToListAsync();

        context.Users.RemoveRange(unverifiedUsers);
        await context.SaveChangesAsync();
    }
}
