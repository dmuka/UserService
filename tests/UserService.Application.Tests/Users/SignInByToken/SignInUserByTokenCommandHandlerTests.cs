using Application.Abstractions.Authentication;
using Application.Users.SignInByToken;
using Domain.RefreshTokens;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Moq;

namespace UserService.Application.Tests.Users.SignInByToken;

[TestFixture]
public class SignInUserByTokenCommandHandlerTests
{
    private readonly Guid _refreshTokenGuid = Guid.CreateVersion7();
    
    private const string ValidToken = "validToken";
    
    private User _user;
    private RefreshToken _validRefreshToken;
    
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
        
        var expireDate = DateTime.UtcNow;
        var validExpireDate = expireDate.AddDays(1);
        
        _validRefreshToken = RefreshToken.Create(
            _refreshTokenGuid,
            ValidToken,
            validExpireDate,
            _user);
        
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