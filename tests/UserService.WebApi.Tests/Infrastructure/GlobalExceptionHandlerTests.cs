using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WebApi.Infrastructure;

namespace UserService.WebApi.Tests.Infrastructure;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private DefaultHttpContext _httpContext;
    private ApplicationException _applicationException;
    private CancellationToken _cancellationToken;
    
    private Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private GlobalExceptionHandler _exceptionHandler;

    [SetUp]
    public void SetUp()
    {
        _httpContext = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };
        _applicationException = new ApplicationException("Test exception");
        _cancellationToken = CancellationToken.None;
        
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _exceptionHandler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Test]
    public async Task TryHandleAsync_ShouldReturnTrue()
    {
        // Arrange
        // Act
        var result = await _exceptionHandler.TryHandleAsync(_httpContext, _applicationException, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
    }

    [Test]
    public async Task TryHandleAsync_ShouldWriteProblemDetailsToResponse()
    {
        // Arrange
        // Act
        await _exceptionHandler.TryHandleAsync(_httpContext, _applicationException, _cancellationToken);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync(_cancellationToken);
        Assert.That(responseBody, Does.Contain("\"status\":500"));
        Assert.That(responseBody, Does.Contain("\"title\":\"Server failure\""));
    }
}