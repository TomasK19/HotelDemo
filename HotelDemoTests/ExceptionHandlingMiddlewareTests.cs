using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using HotelDemo.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelDemoTests
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
        private readonly RequestDelegate _next;
        private readonly ExceptionHandlingMiddleware _middleware;

        public ExceptionHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _next = new RequestDelegate(context => throw new Exception("Test exception"));
            _middleware = new ExceptionHandlingMiddleware(_next, _loggerMock.Object);
        }

        private HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
            return context;
        }

        [Fact]
        public async Task Invoke_WithException_LogsError()
        {
            // Arrange
            var context = CreateHttpContext();

            // Act
            await _middleware.Invoke(context);

            // Assert
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) => v.ToString().Contains("An unexpected error occurred")
                        ),
                        It.Is<Exception>(e => e.Message == "Test exception"),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Invoke_WithArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var next = new RequestDelegate(context =>
                throw new ArgumentException("Invalid argument")
            );
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);
            var context = CreateHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(response);

            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
            Assert.Equal("Invalid argument", result.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Invoke_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var next = new RequestDelegate(context => throw new KeyNotFoundException("Not found"));
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);
            var context = CreateHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(response);

            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
            Assert.Equal("Not found", result.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Invoke_WithGeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var context = CreateHttpContext();

            // Act
            await _middleware.Invoke(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(response);

            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
            Assert.Equal("Test exception", result.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Invoke_WithoutException_CallsNextDelegate()
        {
            // Arrange
            var context = CreateHttpContext();
            var nextCalled = false;
            RequestDelegate next = async ctx =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(nextCalled);
        }
    }
}
