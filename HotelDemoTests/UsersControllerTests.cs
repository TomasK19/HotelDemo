using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using HotelDemo.Controllers;
using HotelDemo.DTO;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelDemoTests;

public class UsersControllerTests
{
    private readonly UsersController _controller;
    private readonly IFixture _fixture;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUserService> _userServiceMock;

    public UsersControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _userServiceMock = _fixture.Freeze<Mock<IUserService>>();
        _tokenServiceMock = _fixture.Freeze<Mock<ITokenService>>();
        _controller = new UsersController(_userServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var userDto = _fixture.Create<UserRegistrationDto>();
        _controller.ModelState.AddModelError("Email", "The Email field is required.");

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var userDto = _fixture.Create<UserRegistrationDto>();
        _userServiceMock
            .Setup(s => s.RegisterUserAsync(userDto))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(new { error = "Error" });
    }

    [Fact]
    public async Task Register_WithValidModelState_ReturnsOk()
    {
        // Arrange
        var userDto = _fixture.Create<UserRegistrationDto>();

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .Be("Registration successful. Please check your email for verification code.");
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var userDto = _fixture.Create<UserLoginDto>();
        _controller.ModelState.AddModelError("Username", "The Username field is required.");

        // Act
        var result = await _controller.Login(userDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userDto = _fixture.Create<UserLoginDto>();
        _userServiceMock
            .Setup(s => s.AuthenticateUserAsync(userDto))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid username or password"));

        // Act
        var result = await _controller.Login(userDto);

        // Assert
        result
            .Should()
            .BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(new { error = "Invalid username or password" });
    }

    [Fact]
    public async Task Login_WithGeneralException_ReturnsBadRequest()
    {
        // Arrange
        var userDto = _fixture.Create<UserLoginDto>();
        _userServiceMock
            .Setup(s => s.AuthenticateUserAsync(userDto))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act
        var result = await _controller.Login(userDto);

        // Assert
        result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(new { error = "An error occurred" });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var userDto = _fixture.Create<UserLoginDto>();
        var user = _fixture
            .Build<User>()
            .Without(u => u.Bookings) // Ensure no circular reference issues
            .Create();
        var token = _fixture.Create<string>();

        _userServiceMock.Setup(s => s.AuthenticateUserAsync(userDto)).ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns(token);

        // Act
        var result = await _controller.Login(userDto);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(new { Token = token, UserId = user.Id });
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var verificationDto = _fixture.Create<EmailVerificationDto>();
        _controller.ModelState.AddModelError("Email", "The Email field is required.");

        // Act
        var result = await _controller.VerifyEmail(verificationDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task VerifyEmail_WhenServiceThrowsUnauthorizedException_ReturnsUnauthorized()
    {
        // Arrange
        var verificationDto = _fixture.Create<EmailVerificationDto>();
        _userServiceMock
            .Setup(s => s.VerifyEmailAsync(verificationDto))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid verification code"));

        // Act
        var result = await _controller.VerifyEmail(verificationDto);

        // Assert
        result
            .Should()
            .BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(new { error = "Invalid verification code" });
    }

    [Fact]
    public async Task VerifyEmail_WithGeneralException_ReturnsBadRequest()
    {
        // Arrange
        var verificationDto = _fixture.Create<EmailVerificationDto>();
        _userServiceMock
            .Setup(s => s.VerifyEmailAsync(verificationDto))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act
        var result = await _controller.VerifyEmail(verificationDto);

        // Assert
        result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(new { error = "An error occurred" });
    }

    [Fact]
    public async Task VerifyEmail_WithValidModelState_ReturnsOk()
    {
        // Arrange
        var verificationDto = _fixture.Create<EmailVerificationDto>();

        // Act
        var result = await _controller.VerifyEmail(verificationDto);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .Be("Email verified successfully.");
    }
}
