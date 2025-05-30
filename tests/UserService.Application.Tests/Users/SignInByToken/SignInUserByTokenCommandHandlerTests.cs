using Application.Abstractions.Authentication;
using Application.Users.SignInByToken;
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

namespace UserService.Application.Tests.Users.SignInByToken;

[TestFixture]
public class SignInUserByTokenCommandHandlerTests
{
    private const int RefreshTokenExpirationInDays = 2;
    private const int RefreshTokenExpirationInDaysOneDay = 1;
    
    private const string ValidToken = "validToken";
    private const string InvalidToken = "invalidToken";

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
    
    private SignInUserByTokenCommandHandler _handler;

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
            ValidToken,
            validExpireDate,
            _user.Id).Value;
        
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _refreshTokenRepositoryMock.Setup(repository => repository.GetTokenAsync(ValidToken, _cancellationToken))
            .ReturnsAsync(_validRefreshToken);
        _refreshTokenRepositoryMock.Setup(repository => repository.GetTokenAsync(InvalidToken, _cancellationToken))
            .ReturnsAsync(Result.Failure<RefreshToken>(RefreshTokenErrors.NotFoundByValue(InvalidToken)));
        
        _tokenProviderMock = new Mock<ITokenProvider>();
        _tokenProviderMock.Setup(provider => provider.CreateAccessTokenAsync(_user, RememberMe, _cancellationToken))
            .Returns(Task.FromResult("newAccessToken"));
        
        _tokenProviderMock.Setup(provider => provider.GetExpirationValue(RefreshTokenExpirationInDays, ExpirationUnits.Day, RememberMe))
            .Returns(DateTime.UtcNow.AddDays(RefreshTokenExpirationInDaysOneDay));
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(repository => repository.GetUserByIdAsync(_user.Id.Value, _cancellationToken))
            .ReturnsAsync(_user);

        _tokenProviderMock.Setup(provider => provider.CreateRefreshToken())
            .Returns("newRefreshToken");
        
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
        
        _handler = new SignInUserByTokenCommandHandler(
            _refreshTokenRepositoryMock.Object, 
            _userRepositoryMock.Object, 
            _tokenProviderMock.Object);
    }

    [Test]
    public async Task Handle_ValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var command = new SignInUserByTokenCommand(ValidToken, RefreshTokenExpirationInDays);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo("newAccessToken"));
            Assert.That(result.Value.SessionId, Is.EqualTo(_refreshTokenGuid));
            _refreshTokenRepositoryMock.Verify(repository => repository.UpdateTokenAsync(It.IsAny<RefreshToken>(), _cancellationToken), Times.Once);
        }
    }

    [Test]
    public async Task Handle_InvalidRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignInUserByTokenCommand(InvalidToken, RefreshTokenExpirationInDays);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Codes.NotFound));
            _refreshTokenRepositoryMock.Verify(repository => repository.UpdateTokenAsync(It.IsAny<RefreshToken>(), _cancellationToken), Times.Never);
        }
    }

    [Test]
    public async Task Handle_WhenUserIdNonExistent_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignInUserByTokenCommand(ValidToken, RefreshTokenExpirationInDays);
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