using Application.Abstractions.Authentication;
using Application.Users.SignInByToken;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Moq;

namespace UserService.Application.Tests.Users.SignInByToken;

[TestFixture]
public class SignInUserByTokenCommandHandlerTests
{
    private const string ValidToken = "validToken";
    private const string ExpiredToken = "expiredToken";
    
    private User _user;
    private RefreshToken _validRefreshToken;
    private RefreshToken _expiredRefreshToken;
    
    private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private Mock<ITokenProvider> _tokenProviderMock;
    private SignInUserByTokenCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _user = User.CreateUser(
            Guid.CreateVersion7(),
            "username",
            "firstName",
            "lastName",
            new PasswordHash("hash"),
            new Email("email@email.com"),
            new List<Role>());
        
        _expiredRefreshToken = new RefreshToken
        {
            Value = ExpiredToken,
            ExpiresUtc = DateTime.UtcNow.AddDays(-1),
            User = _user
        };
        
        _validRefreshToken = new RefreshToken
        {
            Value = ValidToken,
            ExpiresUtc = DateTime.UtcNow.AddDays(1),
            User = _user
        };
        
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _tokenProviderMock = new Mock<ITokenProvider>();

        _tokenProviderMock.Setup(provider => provider.CreateAccessToken(_validRefreshToken.User))
            .Returns("newAccessToken");

        _tokenProviderMock.Setup(provider => provider.CreateRefreshToken())
            .Returns("newRefreshToken");
        
        
        _handler = new SignInUserByTokenCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _tokenProviderMock.Object);
    }

    [Test]
    public void Handle_ExpiredRefreshToken_ShouldThrowApplicationException()
    {
        // Arrange
        var command = new SignInUserByTokenCommand(ExpiredToken);

        _refreshTokenRepositoryMock.Setup(r =>r.GetTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expiredRefreshToken);

        // Act & Assert
        using (Assert.EnterMultipleScope())
        {
            var ex = Assert.ThrowsAsync<ApplicationException>(async () => 
                await _handler.Handle(command, CancellationToken.None));
            Assert.That(ex?.Message, Is.EqualTo("Refresh token has expired."));
        }
    }

    [Test]
    public async Task Handle_ValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var command = new SignInUserByTokenCommand(ValidToken);

        _refreshTokenRepositoryMock.Setup(r => r.GetTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_validRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo("newAccessToken"));
            Assert.That(result.Value.RefreshToken, Is.EqualTo("newRefreshToken"));
        }
    }
}