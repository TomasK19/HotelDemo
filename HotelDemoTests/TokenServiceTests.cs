using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using HotelDemo.Models;
using HotelDemo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

public class TokenServiceTests
{
    private Mock<IConfiguration> CreateMockConfiguration()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.SetupGet(c => c["Jwt:Key"]).Returns("supersecretkeysupersecretkey1234"); // 32 bytes key
        mockConfig.SetupGet(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        mockConfig.SetupGet(c => c["Jwt:Audience"]).Returns("TestAudience");
        mockConfig.SetupGet(c => c["Jwt:ExpiresInMinutes"]).Returns("60");

        return mockConfig;
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Arrange
        var mockConfig = CreateMockConfiguration();
        var tokenService = new TokenService(mockConfig.Object);

        var user = new User
        {
            Id = 1,
            Email = "testuser@example.com",
            Username = "testuser"
        };

        // Act
        var token = tokenService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(mockConfig.Object["Jwt:Key"])
        );
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = mockConfig.Object["Jwt:Issuer"],
            ValidAudience = mockConfig.Object["Jwt:Audience"],
            IssuerSigningKey = securityKey
        };

        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(
            token,
            tokenValidationParameters,
            out validatedToken
        );

        principal.Should().NotBeNull();
        principal
            .Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value.Should()
            .Be(user.Email);
        principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value.Should().Be(user.Id.ToString());
        principal
            .Claims.FirstOrDefault(c => c.Type == "username")
            ?.Value.Should()
            .Be(user.Username);
    }
}
