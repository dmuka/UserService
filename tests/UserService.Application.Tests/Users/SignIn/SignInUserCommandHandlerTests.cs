using Application.Abstractions.Authentication;
using Application.Users.SignIn;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
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
    
    private IList<Role> _roles;
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private User _existingUser;
    
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<ITokenProvider> _tokenProviderMock;
    private SignInUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roles = new List<Role> { Role.Create(Guid.CreateVersion7(), "Role") };
        _existingUser = User.Create(
            Guid.CreateVersion7(),
            ExistingUsername,
            "firstName",
            "lastName",
            "hash",
            "email@email.com",
            _roles.Select(role => role.Id).ToList(),
            new List<UserPermissionId>()).Value;
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(ExistingUsername, _cancellationToken))
            .ReturnsAsync(_existingUser);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(ExistingEmail, _cancellationToken))
            .ReturnsAsync(_existingUser);
        
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _passwordHasherMock.Setup(ph => ph.CheckPassword(CorrectPassword, _existingUser.PasswordHash))
            .Returns(true);
        
        _tokenProviderMock = new Mock<ITokenProvider>();
        _tokenProviderMock.Setup(tp => tp.CreateAccessTokenAsync(_existingUser, _cancellationToken))
            .Returns(Task.FromResult(AccessToken));
        _tokenProviderMock.Setup(tp => tp.CreateRefreshToken())
            .Returns(RefreshTokenValue);
        
        _handler = new SignInUserCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenProviderMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExistByUsername()
    {
        // Arrange
        var command = new SignInUserCommand(NonExistingUsername, CorrectPassword, RefreshTokenExpirationInDays);
        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync((User)null!);

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
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, RefreshTokenExpirationInDays, NonExistingEmail);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.NotFoundByEmail(command.Email!)));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnWrongPassword_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new SignInUserCommand(ExistingUsername, WrongPassword, RefreshTokenExpirationInDays);
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
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, RefreshTokenExpirationInDays);

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
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, RefreshTokenExpirationInDays, ExistingEmail);

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