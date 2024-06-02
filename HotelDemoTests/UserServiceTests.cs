using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using FluentAssertions;
using HotelDemo.Data;
using HotelDemo.DTO;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HotelDemoTests
{
    public class UserServiceTests
    {
        private readonly HotelBookingContext _context;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly IFixture _fixture;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<HotelBookingContext>()
                .UseInMemoryDatabase("HotelBookingTestDb")
                .Options;

            _context = new HotelBookingContext(options);

            _emailServiceMock = new Mock<IEmailService>();
            _userService = new UserService(_context, _emailServiceMock.Object);

            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Remove the default ThrowingRecursionBehavior and add OmitOnRecursionBehavior
            _fixture
                .Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task RegisterUserAsync_ValidUser_SendsEmail()
        {
            // Arrange
            var userDto = _fixture
                .Build<UserRegistrationDto>()
                .With(u => u.Email, "test@example.com")
                .With(u => u.Username, "testuser")
                .With(u => u.Password, "Password123")
                .Create();

            // Act
            await _userService.RegisterUserAsync(userDto);

            // Assert
            _emailServiceMock.Verify(
                service => service.SendVerificationEmailAsync(userDto.Email, It.IsAny<string>()),
                Times.Once
            );

            var registeredUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == userDto.Email
            );
            registeredUser.Should().NotBeNull();
            registeredUser.Username.Should().Be(userDto.Username);
            registeredUser.IsVerified.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserAsync_ExistingEmailOrUsername_ThrowsException()
        {
            // Arrange
            var user = _fixture
                .Build<User>()
                .Without(u => u.Id)
                .With(u => u.Email, "test@example.com")
                .With(u => u.Username, "testuser")
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("Password123"))
                .With(u => u.IsVerified, true)
                .Create();
            user.Id = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = _fixture
                .Build<UserRegistrationDto>()
                .With(u => u.Email, "test@example.com")
                .With(u => u.Username, "testuser")
                .With(u => u.Password, "Password123")
                .Create();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _userService.RegisterUserAsync(userDto)
            );
        }

        [Fact]
        public async Task AuthenticateUserAsync_ValidUser_ReturnsUser()
        {
            // Arrange
            var user = _fixture
                .Build<User>()
                .Without(u => u.Id)
                .With(u => u.Email, "test@example.com")
                .With(u => u.Username, "testuser")
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("Password123"))
                .With(u => u.IsVerified, true)
                .Create();
            user.Id = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = _fixture
                .Build<UserLoginDto>()
                .With(u => u.Username, "testuser")
                .With(u => u.Password, "Password123")
                .Create();

            // Act
            var result = await _userService.AuthenticateUserAsync(userDto);

            // Assert
            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.Username.Should().Be(user.Username);
        }

        [Fact]
        public async Task AuthenticateUserAsync_InvalidUsername_ThrowsException()
        {
            // Arrange
            var userDto = _fixture
                .Build<UserLoginDto>()
                .With(u => u.Username, "invaliduser")
                .With(u => u.Password, "Password123")
                .Create();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _userService.AuthenticateUserAsync(userDto)
            );
        }

        [Fact]
        public async Task AuthenticateUserAsync_InvalidPassword_ThrowsException()
        {
            // Arrange
            var user = _fixture
                .Build<User>()
                .Without(u => u.Id)
                .With(u => u.Email, "test@example.com")
                .With(u => u.Username, "testuser")
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("Password123"))
                .With(u => u.IsVerified, true)
                .Create();
            user.Id = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = _fixture
                .Build<UserLoginDto>()
                .With(u => u.Username, "testuser")
                .With(u => u.Password, "WrongPassword")
                .Create();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _userService.AuthenticateUserAsync(userDto)
            );
        }

        [Fact]
        public async Task AuthenticateUserAsync_UnverifiedUser_ThrowsException()
        {
            // Arrange
            var user = _fixture
                .Build<User>()
                .Without(u => u.Id)
                .With(u => u.Email, "test@example.com")
                .With(u => u.Username, "testuser")
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("Password123"))
                .With(u => u.IsVerified, false)
                .Create();
            user.Id = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = _fixture
                .Build<UserLoginDto>()
                .With(u => u.Username, "testuser")
                .With(u => u.Password, "Password123")
                .Create();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _userService.AuthenticateUserAsync(userDto)
            );
        }

        [Fact]
        public async Task VerifyEmailAsync_ValidCode_UpdatesUser()
        {
            // Arrange
            var user = _fixture
                .Build<User>()
                .Without(u => u.Id) // Remove the default Id generation
                .With(u => u.Email, "test@example.com")
                .With(u => u.VerificationCode, "123456")
                .With(u => u.IsVerified, false)
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("Password123"))
                .Create();
            user.Id = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var verificationDto = _fixture
                .Build<EmailVerificationDto>()
                .With(v => v.Email, "test@example.com")
                .With(v => v.VerificationCode, "123456")
                .Create();

            // Act
            await _userService.VerifyEmailAsync(verificationDto);

            // Assert
            var verifiedUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == verificationDto.Email
            );
            verifiedUser.IsVerified.Should().BeTrue();
            verifiedUser.VerificationCode.Should().BeNull();
        }

        [Fact]
        public async Task VerifyEmailAsync_InvalidVerificationCode_ThrowsException()
        {
            // Arrange
            var user = _fixture
                .Build<User>()
                .Without(u => u.Id) // Remove the default Id generation
                .With(u => u.Email, "test@example.com")
                .With(u => u.VerificationCode, "123456")
                .With(u => u.IsVerified, false)
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("Password123"))
                .Create();
            user.Id = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var verificationDto = _fixture
                .Build<EmailVerificationDto>()
                .With(v => v.Email, "test@example.com")
                .With(v => v.VerificationCode, "wrongcode")
                .Create();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _userService.VerifyEmailAsync(verificationDto)
            );
        }
    }
}
