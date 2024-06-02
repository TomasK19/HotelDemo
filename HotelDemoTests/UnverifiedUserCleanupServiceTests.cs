using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HotelDemo.Data;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UnverifiedUserCleanupServiceTests
{
    private HotelBookingContext CreateContextWithUsers(params User[] users)
    {
        var options = new DbContextOptionsBuilder<HotelBookingContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new HotelBookingContext(options);
        context.Users.AddRange(users);
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task CleanupUnverifiedUsersAsync_ShouldRemoveUnverifiedUsers_Expired()
    {
        // Arrange
        var expiredUser = new User
        {
            Id = 1,
            Email = "expired@example.com",
            Username = "expireduser",
            PasswordHash = "hashedpassword",
            VerificationCode = "123456",
            IsVerified = false,
            RegistrationTimestamp = DateTime.UtcNow - TimeSpan.FromMinutes(11)
        };

        var validUser = new User
        {
            Id = 2,
            Email = "valid@example.com",
            Username = "validuser",
            PasswordHash = "hashedpassword",
            VerificationCode = "123456",
            IsVerified = false,
            RegistrationTimestamp = DateTime.UtcNow - TimeSpan.FromMinutes(5)
        };

        var context = CreateContextWithUsers(expiredUser, validUser);
        var service = new UnverifiedUserCleanupService(context);

        // Act
        await service.CleanupUnverifiedUsersAsync();

        // Assert
        var users = await context.Users.ToListAsync();
        users.Should().ContainSingle(u => u.Email == "valid@example.com");
        users.Should().NotContain(u => u.Email == "expired@example.com");
    }

    [Fact]
    public async Task CleanupUnverifiedUsersAsync_ShouldNotRemoveVerifiedUsers()
    {
        // Arrange
        var verifiedUser = new User
        {
            Id = 3,
            Email = "verified@example.com",
            Username = "verifieduser",
            PasswordHash = "hashedpassword",
            VerificationCode = "123456",
            IsVerified = true,
            RegistrationTimestamp = DateTime.UtcNow - TimeSpan.FromMinutes(15)
        };

        var context = CreateContextWithUsers(verifiedUser);
        var service = new UnverifiedUserCleanupService(context);

        // Act
        await service.CleanupUnverifiedUsersAsync();

        // Assert
        var users = await context.Users.ToListAsync();
        users.Should().ContainSingle(u => u.Email == "verified@example.com");
    }
}
