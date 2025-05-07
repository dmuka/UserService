using Application.Abstractions.Behaviors;
using Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace UserService.Application.Tests.Abstractions.Behaviors
{
    [TestFixture]
    public class RequestLoggingPipelineBehaviorTests
    {
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
        private Mock<ILogger<RequestLoggingPipelineBehavior<TestRequest, TestResponse>>> _loggerMock;
        private Mock<RequestHandlerDelegate<TestResponse>> _nextMock;
        private RequestLoggingPipelineBehavior<TestRequest, TestResponse> _behavior;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<RequestLoggingPipelineBehavior<TestRequest, TestResponse>>>();
            _loggerMock
                .Setup(logger => logger.Log(
                    LogLevel.Information, 
                    It.IsAny<EventId>(), 
                    It.IsAny<It.IsAnyType>(), 
                    It.IsAny<Exception>(), 
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>() 
                ))
                .Verifiable();
            
            _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
            _behavior = new RequestLoggingPipelineBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        }

        [Test]
        public async Task Handle_ShouldLogStartAndEnd_WhenRequestIsSuccessful()
        {
            // Arrange
            var request = new TestRequest();
            var response = new TestResponse(true, Error.None);
            _nextMock.Setup(handler => handler(_cancellationToken)).ReturnsAsync(response);

            // Act
            var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(response));
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, type) => obj.ToString()!.Contains("Start processing request TestRequest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, type) => obj.ToString()!.Contains("End processing request TestRequest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);        
        }

        [Test]
        public async Task Handle_ShouldLogError_WhenRequestFails()
        {
            // Arrange
            var request = new TestRequest();
            var response = new TestResponse(false, new Error("Code","Some error", ErrorType.Failure));
            _nextMock.Setup(n => n(_cancellationToken)).ReturnsAsync(response);

            // Act
            var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(response));
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, type) => obj.ToString()!.Contains("Start processing request TestRequest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, type) => obj.ToString()!.Contains("End processing request TestRequest with error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);        
        }

        public class TestRequest { }

        public class TestResponse(bool isSuccess, Error error) : Result(isSuccess, error)
        {
            public new bool IsSuccess { get; set; } = isSuccess;
            public new string Error { get; set; } = error.Description;
        }
    }
}