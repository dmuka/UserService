using Application.Users.SignIn;
using Application.Users.SignInByToken;
using Application.Users.SignUp;
using Core;
using Domain.Users;
using Grpc.Core;
using Grpc.Protos;
using Grpc.Services;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SignInResponse = Application.Users.SignIn.SignInResponse;

namespace UserService.Grpc.Tests;

[TestFixture]
public class UserGrpcServiceTests
{
    private const string Username = "testuser";
    private const string Email = "test@example.com";
    private const string Password = "password";
    private const string AccessToken = "access-token";
    private const string RefreshToken = "refresh-token";
    private readonly Guid SessionId = Guid.CreateVersion7();
    
    private readonly SignUpRequest _signUpRequest = new()
    {
        Username = Username, 
        Email = Email, 
        FirstName = "Test", 
        LastName = "User", 
        Password = Password
    };
    private readonly SignInRequest _signInRequest = new()
    {
        Username = Username, 
        Password = Password, 
        Email = Email
    };
    private readonly SignInByTokenRequest _signInByTokenRequest = new()
    {
        RefreshToken = RefreshToken
    };
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<ServerCallContext> _serverCallContextMock;
    
    private Mock<ILogger<UserGrpcService>> _loggerMock;
    private Mock<IOptions<AuthOptions>> _authOptionsMock;
    private Mock<ISender> _senderMock;
    
    private UserGrpcService _service;

    [SetUp]
    public void Setup()
    {
        _serverCallContextMock = new Mock<ServerCallContext>();
        
        _loggerMock = new Mock<ILogger<UserGrpcService>>();
        _senderMock = new Mock<ISender>();
        
        _authOptionsMock = new Mock<IOptions<AuthOptions>>();
        _authOptionsMock.Setup(opt => opt.Value).Returns(new AuthOptions
        {
            Secret = "secret",
            Issuer = "issuer",
            Audience = "audience",
            AccessTokenExpirationInMinutes = 1,
            AccessTokenCookieExpirationInMinutes = 1,
            SessionIdCookieExpirationInHours = 1,
            RefreshTokenExpirationInDays = 1,
            ResetPasswordTokenExpirationInMinutes = 10
        });
        
        _service = new UserGrpcService(_authOptionsMock.Object, _loggerMock.Object, _senderMock.Object);
    }

    [Test]
    public async Task SignUp_ShouldReturnSignUpResponse_WhenSuccessful()
    {
        // Arrange
        var result = Result.Success(Guid.CreateVersion7());
        _senderMock.Setup(s => s.Send(It.IsAny<SignUpUserCommand>(), _cancellationToken))
            .ReturnsAsync(result);

        // Act
        var response = await _service.SignUp(_signUpRequest, _serverCallContextMock.Object);

        // Assert
        Assert.That(response.Id, Is.EqualTo(result.Value.ToString()));
    }

    [Test]
    public void SignUp_ShouldThrowRpcException_WhenFailed()
    {
        // Arrange
        var result = Result.Failure<Guid>(UserErrors.UsernameAlreadyExists);
        _senderMock.Setup(s => s.Send(It.IsAny<SignUpUserCommand>(), _cancellationToken))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(
            () => _service.SignUp(_signUpRequest, _serverCallContextMock.Object));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.Internal));
    }

    [Test]
    public async Task SignIn_ShouldReturnSignInResponse_WhenSuccessful()
    {
        // Arrange
        var result = Result.Success(new SignInResponse(AccessToken, SessionId));
        _senderMock.Setup(s => s.Send(It.IsAny<SignInUserCommand>(), _cancellationToken))
            .ReturnsAsync(result);

        // Act
        var response = await _service.SignIn(_signInRequest, _serverCallContextMock.Object);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(response.AccessToken, Is.EqualTo(AccessToken));
            Assert.That(response.SessionId, Is.EqualTo(SessionId.ToString()));
        }
    }

    [Test]
    public void SignIn_ShouldThrowRpcException_WhenFailed()
    {
        // Arrange
        var result = Result.Failure<SignInResponse>(UserErrors.NotFoundByUsername(_signInRequest.Username));
        _senderMock.Setup(s => s.Send(It.IsAny<SignInUserCommand>(), _cancellationToken))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(
            () => _service.SignIn(_signInRequest, _serverCallContextMock.Object));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
    }

    [Test]
    public async Task SignInByToken_ShouldReturnSignInResponse_WhenSuccessful()
    {
        // Arrange
        var result = Result.Success(new SignInUserByTokenResponse(AccessToken, SessionId));
        _senderMock.Setup(s => s.Send(It.IsAny<SignInUserByTokenCommand>(), _cancellationToken))
            .ReturnsAsync(result);

        // Act
        var response = await _service.SignInByToken(_signInByTokenRequest, _serverCallContextMock.Object);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(response.AccessToken, Is.EqualTo(AccessToken));
            Assert.That(response.SessionId, Is.EqualTo(SessionId.ToString()));
        }
    }

    [Test]
    public void SignInByToken_ShouldThrowRpcException_WhenFailed()
    {
        // Arrange
        var result = Result.Failure<SignInUserByTokenResponse>(UserErrors.WrongPassword());
        _senderMock.Setup(s => s.Send(It.IsAny<SignInUserByTokenCommand>(), _cancellationToken))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(
            () => _service.SignInByToken(_signInByTokenRequest, _serverCallContextMock.Object));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
    }
}