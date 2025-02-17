using Application.Abstractions.Authentication;
using Application.Users.SignUp;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Moq;
using RoleConstants = Domain.Roles.Constants.Roles;

namespace UserService.Application.Tests.Users.SignUp;

[TestFixture]
public class SignUpUserCommandHandlerTests
{
    private const string Username = "User";
    private const string ExistingUsername = "existingUser";
    private const string FirstName = "firstName";
    private const string LastName = "lastName";
    private const string Email = "email@email.com";
    private const string ExistingEmail = "exiatingEmail@email.com";
    private const string Password = "email@email.com";

    private User _user;
    
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private SignUpUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _user = User.CreateUser(
            Guid.CreateVersion7(),
            Username,
            "firstName",
            "lastName",
            new PasswordHash("hash"),
            new Email(Email),
            new List<Role>());
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(r => r.IsUsernameExistsAsync(Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.IsEmailExistsAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.IsUsernameExistsAsync(ExistingUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.IsEmailExistsAsync(ExistingEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.CreateVersion7);
        
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _roleRepositoryMock.Setup(repo => repo.GetRoleByNameAsync(RoleConstants.DefaultUserRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Role.CreateRole(Guid.CreateVersion7(), RoleConstants.DefaultUserRole));
        
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _passwordHasherMock.Setup(hasher => hasher.GetHash(Password))
            .Returns("hash");
        
        _handler = new SignUpUserCommandHandler(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Test]
    public async Task Handle_UsernameAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignUpUserCommand(ExistingUsername, Email, FirstName, LastName, Password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.UsernameAlreadyExists));
        }
    }

    [Test]
    public async Task Handle_EmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignUpUserCommand(Username, ExistingEmail, FirstName, LastName, Password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.EmailAlreadyExists));
        }
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new SignUpUserCommand(Username, Email, FirstName, LastName, Password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
        }
    }
}