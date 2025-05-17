using Application.Abstractions.Authentication;
using Application.Users.SignIn;
using Domain.RefreshTokens;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.RoleNames;
using Microsoft.Extensions.Logging;
using Moq;

namespace UserService.Application.Tests.Users.SignIn;

[TestFixture]
public class SignInUserCommandHandlerTests
{
    private const int RefreshTokenExpirationInDays = 1;
    
    private const string ExistingUsername = "existingUser";
    private const string NonExistingUsername = "nonExistingUser";
    
    private const string ExistingEmail = "existingEmail@email.com";
    private const string NonExistingEmail = "nonExistingEmail@email.com";
    
    private const string CorrectPassword = "password";
    private const string WrongPassword = "poopypassword";
        
    private const string AccessToken = "accessToken";
    private const string RefreshTokenValue = "refreshTokenValue";

    private const bool RememberMe = false;

    private IList<Role> _roles;
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private User _existingUser;
    
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<ITokenProvider> _tokenProviderMock;
    private Mock<ITotpProvider> _totpProviderMock;
    private Mock<IRecoveryCodesProvider> _recoveryCodesProviderMock;
    private Mock<ILogger<SignInUserCommandHandler>> _loggerMock;
    
    private SignInUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roles = new List<Role> { Role.Create(Guid.CreateVersion7(), "Role").Value };
        _existingUser = User.Create(
            Guid.CreateVersion7(),
            ExistingUsername,
            "firstName",
            "lastName",
            "hash",
            "email@email.com",
            _roles.Select(role => RoleName.Create(role.Name).Value).ToList(),
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(ExistingUsername, _cancellationToken))
            .ReturnsAsync(_existingUser);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(ExistingEmail, _cancellationToken))
            .ReturnsAsync(_existingUser);
        
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _refreshTokenRepositoryMock.Setup(repo => repo.GetTokenByUserAsync(_existingUser, _cancellationToken))
            .ReturnsAsync(RefreshToken.Create(Guid.CreateVersion7(), RefreshTokenValue, DateTime.UtcNow.AddDays(1), _existingUser.Id).Value);
        
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _passwordHasherMock.Setup(ph => ph.CheckPassword(CorrectPassword, _existingUser.PasswordHash))
            .Returns(true);
        
        _tokenProviderMock = new Mock<ITokenProvider>();
        _tokenProviderMock.Setup(tp => tp.CreateAccessTokenAsync(_existingUser, RememberMe, _cancellationToken))
            .Returns(Task.FromResult(AccessToken));
        _tokenProviderMock.Setup(tp => tp.CreateRefreshToken())
            .Returns(RefreshTokenValue);
        
        _totpProviderMock = new Mock<ITotpProvider>();
        
        _recoveryCodesProviderMock = new Mock<IRecoveryCodesProvider>();
        
        _loggerMock = new Mock<ILogger<SignInUserCommandHandler>>();
        
        _handler = new SignInUserCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenProviderMock.Object,
            _totpProviderMock.Object,
            _recoveryCodesProviderMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExistByUsername()
    {
        // Arrange
        var command = new SignInUserCommand(NonExistingUsername, CorrectPassword, RememberMe, RefreshTokenExpirationInDays);
        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.NotFoundByUsername(command.Username)));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExistByEmail()
    {
        // Arrange
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, RememberMe, RefreshTokenExpirationInDays, Email: NonExistingEmail);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.NotFoundByEmail(command.Email ?? "")));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnWrongPassword_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new SignInUserCommand(ExistingUsername, WrongPassword, RememberMe, RefreshTokenExpirationInDays);
        _passwordHasherMock.Setup(ph => ph.CheckPassword(command.Password, _existingUser.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.WrongPassword()));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnSignInResponse_WhenSignInIsSuccessful()
    {
        // Arrange
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, RememberMe, RefreshTokenExpirationInDays);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo(AccessToken));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnSignInResponse_WhenSignInByEmailIsSuccessful()
    {
        // Arrange
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, RememberMe, RefreshTokenExpirationInDays, ExistingEmail);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo(AccessToken));
        }
    }
}