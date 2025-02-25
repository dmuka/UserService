using Application.Abstractions.Behaviors;
using Core;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace UserService.Application.Tests.Abstractions.Behaviors
{
    [TestFixture]
    public class ValidationPipelineBehaviorTests
    {
        private const string ErrorMessage = "Error message";
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
        private Mock<IValidator<TestRequest>> _validatorMock;
        private ValidationPipelineBehavior<TestRequest, Result<TestResponse>> _behavior;
        private Mock<RequestHandlerDelegate<Result<TestResponse>>> _nextMock;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<TestRequest>>();
            _nextMock = new Mock<RequestHandlerDelegate<Result<TestResponse>>>();
            _behavior = new ValidationPipelineBehavior<TestRequest, Result<TestResponse>>([ _validatorMock.Object ]);
        }

        [Test]
        public async Task Handle_ShouldReturnNext_WhenValidationSucceeds()
        {
            // Arrange
            var request = new TestRequest();
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                          .ReturnsAsync(new ValidationResult());

            var expectedResponse = new Result<TestResponse>(new TestResponse(), true, Error.None);
            _nextMock.Setup(n => n()).ReturnsAsync(expectedResponse);

            // Act
            var response = await _behavior.Handle(request, _nextMock.Object, _cancellationToken);

            // Assert
            Assert.That(response, Is.EqualTo(expectedResponse));
            _nextMock.Verify(n => n(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var request = new TestRequest();
            var validationFailures = new[]
            {
                new ValidationFailure("Property", ErrorMessage)
            };
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), _cancellationToken))
                          .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var response = await _behavior.Handle(request, _nextMock.Object, _cancellationToken);
            using (Assert.EnterMultipleScope())
            {

                // Assert
                Assert.That(response.IsFailure, Is.True);
                Assert.That(response.Error.Type, Is.EqualTo(ErrorType.Validation));
            }
        }

        public class TestRequest { }
        public class TestResponse { }
    }
}