using Application.Abstractions.Authentication;
using Application.Users.SignIn;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Moq;

namespace UserService.Application.Tests.Users.SignIn;

[TestFixture]
public class SignInUserCommandHandlerTests
{
    private const string ExistingUsername = "existingUser";
    private const string NonExistingUsername = "nonExistingUser";
    
    private const string ExistingEmail = "existingEmail@email.com";
    private const string NonExistingEmail = "nonExistingEmail@email.com";
    
    private const string CorrectPassword = "password";
    private const string WrongPassword = "poopypassword";
        
    private const string AccessToken = "accessToken";
    private const string RefreshTokenValue = "refreshTokenValue";
    
    private User _existingUser;
    
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<ITokenProvider> _tokenProviderMock;
    private SignInUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _existingUser = User.CreateUser(
            Guid.CreateVersion7(),
            ExistingUsername,
            "firstName",
            "lastName",
            new PasswordHash("hash"),
            new Email("email@email.com"),
            new List<Role>());
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(ExistingUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingUser);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(ExistingEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingUser);
        
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _passwordHasherMock.Setup(ph => ph.CheckPassword(CorrectPassword, _existingUser.PasswordHash))
            .Returns(true);
        
        _tokenProviderMock = new Mock<ITokenProvider>();
        _tokenProviderMock.Setup(tp => tp.CreateAccessToken(_existingUser))
            .Returns(AccessToken);
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
        var command = new SignInUserCommand(NonExistingUsername, CorrectPassword);
        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new SignInUserCommand(ExistingUsername, NonExistingEmail, ExistingEmail);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new SignInUserCommand(ExistingUsername, WrongPassword);
        _passwordHasherMock.Setup(ph => ph.CheckPassword(command.Password, _existingUser.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo(AccessToken));
            Assert.That(result.Value.RefreshToken, Is.EqualTo(RefreshTokenValue));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnSignInResponse_WhenSignInByEmailIsSuccessful()
    {
        // Arrange
        var command = new SignInUserCommand(ExistingUsername, CorrectPassword, ExistingEmail);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.AccessToken, Is.EqualTo(AccessToken));
            Assert.That(result.Value.RefreshToken, Is.EqualTo(RefreshTokenValue));
        }
    }
}