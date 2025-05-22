using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.RoleNames;
using Infrastructure.Authentication;
using Infrastructure.Options.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace UserService.Infrastructure.Tests.Authentication;

[TestFixture]
public class TokenProviderTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private const bool RememberMe = false;

    private Mock<IOptions<AuthOptions>> _authOptionsMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private Mock<IServiceScope> _serviceScopeMock;
    private Mock<ILogger<TokenProvider>> _loggerMock;
    
    private TokenProvider _tokenProvider;

    [SetUp]
    public void SetUp()
    {
        _authOptionsMock = new Mock<IOptions<AuthOptions>>();
        _authOptionsMock.Setup(x => x.Value).Returns(new AuthOptions
        {
            Secret = "12345678901234567890123456789012",
            AccessTokenExpirationInMinutes = 10,
            AccessTokenCookieExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 30,
            SessionIdCookieExpirationInHours = 12,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ResetPasswordTokenExpirationInMinutes = 10,
            EmailConfirmationTokenExpirationInHours = 10
        });
        
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _roleRepositoryMock.Setup(repository => repository.GetRolesByUserIdAsync(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync(new List<Role>());
        
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProviderMock.Setup(provider => provider.GetService(typeof(IRoleRepository))).Returns(_roleRepositoryMock.Object);
        
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
    
        _loggerMock = new Mock<ILogger<TokenProvider>>();
        
        _tokenProvider = new TokenProvider(_authOptionsMock.Object, _serviceScopeFactoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateAccessToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = User.Create(
            Guid.CreateVersion7(),
            "testuser",
            "firstName",
            "lastName",
            "hashedPassword",
            "testuser@example.com",
            new List<RoleName> { RoleName.Create("Role") },
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;

        // Act
        var token = await _tokenProvider.CreateAccessTokenAsync(user, RememberMe, _cancellationToken);

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