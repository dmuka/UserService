using Application.Abstractions.Authentication;
using Application.Users.SignInBySessionId;
using Core;
using Domain.RefreshTokens;
using Domain.RefreshTokens.Constants;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.RoleNames;
using Infrastructure.Options.Authentication;
using Microsoft.Extensions.Options;
using Moq;

namespace UserService.Application.Tests.Users.SignInBySessionId;

[TestFixture]
public class SignInBySessionIdCommandHandlerTests
{
    private readonly Guid _sessionId = Guid.CreateVersion7();
    private readonly Guid _invalidSessionId = Guid.CreateVersion7();
    
    private const int RefreshTokenExpirationInDays = 2;
    private const int RefreshTokenExpirationInDaysOneDay = 1;
    
    private const string ValidRefreshTokenValue = "validRefreshToken";
    private const string AccessTokenValue = "accessToken";

    private const bool RememberMe = false;

    private readonly Guid _refreshTokenGuid = Guid.CreateVersion7();
    private IList<Role> _roles;
    private User _user;
    private RefreshToken _validRefreshToken;
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IOptions<AuthOptions>> _authOptionsMock;
    private Mock<ITokenProvider> _tokenProviderMock;
    
    private SignInUserBySessionIdCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roles = new List<Role> { Role.Create(Guid.CreateVersion7(), "Role").Value };
        _user = User.Create(
            Guid.CreateVersion7(),
            "username",
            "firstName",
            "lastName",
            "hash",
            "email@email.com",
            _roles.Select(role => RoleName.Create(role.Name).Value).ToList(),
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;
        
        var expireDate = DateTime.UtcNow;
        var validExpireDate = expireDate.AddDays(1);
        
        _validRefreshToken = RefreshToken.Create(
            _refreshTokenGuid,
            ValidRefreshTokenValue,
            validExpireDate,
            _user.Id).Value;
        
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _refreshTokenRepositoryMock.Setup(repository => repository.GetTokenByIdAsync(_sessionId, _cancellationToken))
            .ReturnsAsync(_validRefreshToken);
        _refreshTokenRepositoryMock.Setup(repository => repository.GetTokenByIdAsync(_invalidSessionId, _cancellationToken))
            .ReturnsAsync(Result.Failure<RefreshToken>(RefreshTokenErrors.NotFound(_sessionId)));
        
        _tokenProviderMock = new Mock<ITokenProvider>();
        _tokenProviderMock.Setup(provider => provider.CreateAccessTokenAsync(_user, RememberMe, _cancellationToken))
            .Returns(Task.FromResult(AccessTokenValue));
        
        _tokenProviderMock.Setup(provider => provider.GetExpirationValue(RefreshTokenExpirationInDays, ExpirationUnits.Day, RememberMe))
            .Returns(DateTime.UtcNow.AddDays(RefreshTokenExpirationInDaysOneDay));
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(repository => repository.GetUserByIdAsync(_user.Id.Value, _cancellationToken))
            .ReturnsAsync(_user);

        _tokenProviderMock.Setup(provider => provider.CreateRefreshToken())
            .Returns(ValidRefreshTokenValue);
        
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
            ResetPasswordTokenExpirationInMinutes = 10,
            EmailConfirmationTokenExpirationInHours = 10
        });
        
        _handler = new SignInUserBySessionIdCommandHandler(
            _refreshTokenRepositoryMock.Object, 
            _userRepositoryMock.Object, 
            _tokenProviderMock.Object);
    }

    [Test]
    public async Task Handle_ValidRefreshToken_ShouldReturnNewAccessToken()
    {
        // Arrange
        var command = new SignInUserBySessionIdCommand(_sessionId);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo(AccessTokenValue));
        }
    }

    [Test]
    public async Task Handle_WhenSessionIdInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignInUserBySessionIdCommand(_invalidSessionId);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Codes.NotFound));
            _tokenProviderMock.Verify(provider => provider.CreateAccessTokenAsync(It.IsAny<User>(), RememberMe, _cancellationToken), Times.Never);
        }
    }

    [Test]
    public async Task Handle_WhenUserIdNonExistent_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignInUserBySessionIdCommand(_sessionId);
        _userRepositoryMock.Setup(repository => repository.GetUserByIdAsync(_validRefreshToken.UserId, _cancellationToken))
            .ReturnsAsync((User)null!);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Domain.Users.Constants.Codes.NotFound));
            _tokenProviderMock.Verify(provider => provider.CreateAccessTokenAsync(It.IsAny<User>(), RememberMe, _cancellationToken), Times.Never);
        }
    }
}