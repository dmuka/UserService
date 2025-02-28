using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Authentication;
using Infrastructure.Options.Authentication;
using Microsoft.Extensions.Options;
using Moq;

namespace UserService.Infrastructure.Tests.Authentication;

[TestFixture]
public class TokenProviderTests
{
    private Mock<IOptions<AuthOptions>> _authOptionsMock;
    private TokenProvider _tokenProvider;

    [SetUp]
    public void SetUp()
    {
        _authOptionsMock = new Mock<IOptions<AuthOptions>>();
        _authOptionsMock.Setup(x => x.Value).Returns(new AuthOptions
        {
            Secret = "12345678901234567890123456789012",
            ExpirationInMinutes = 60,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        });

        _tokenProvider = new TokenProvider(_authOptionsMock.Object);
    }

    [Test]
    public void CreateAccessToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = User.CreateUser(
            Guid.CreateVersion7(),
            "testuser",
            "firstName",
            "lastName",
            new PasswordHash("hashedPassword"),
            new Email("testuser@example.com"),
            new List<RoleId> { new (Guid.CreateVersion7()) },
            new List<UserPermissionId>());

        // Act
        var token = _tokenProvider.CreateAccessToken(user);

        // Assert
        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public void CreateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _tokenProvider.CreateRefreshToken();

        // Assert
        Assert.That(refreshToken, Is.Not.Empty);
    }
}