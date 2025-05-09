using Application.Abstractions.Authentication;
using Application.Users.SignUp;
using Domain;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
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
    
    private IList<Role> _roles;
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private User _user;
    
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    
    private SignUpUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roles = new List<Role> { Role.Create(Guid.CreateVersion7(), "Role").Value };
        _user = User.Create(
            Guid.CreateVersion7(),
            Username,
            "firstName",
            "lastName",
            "hash",
            Email,
            _roles.Select(role => role.Id).ToList(),
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;
        
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock.Setup(r => r.IsUsernameExistsAsync(Username, _cancellationToken))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.IsEmailExistsAsync(Email, _cancellationToken))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.IsUsernameExistsAsync(ExistingUsername, _cancellationToken))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.IsEmailExistsAsync(ExistingEmail, _cancellationToken))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.AddUserAsync(It.IsAny<User>(), _cancellationToken))
            .ReturnsAsync(Guid.CreateVersion7);
        
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _roleRepositoryMock.Setup(repo => repo.GetRoleByNameAsync(RoleConstants.DefaultUserRole, _cancellationToken))
            .ReturnsAsync(Role.Create(Guid.CreateVersion7(), RoleConstants.DefaultUserRole).Value);
        
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _passwordHasherMock.Setup(hasher => hasher.GetHash(Password))
            .Returns("hash");

        _eventDispatcherMock = new Mock<IEventDispatcher>();
        
        _handler = new SignUpUserCommandHandler(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _passwordHasherMock.Object,
            _eventDispatcherMock.Object);
    }

    [Test]
    public async Task Handle_UsernameAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new SignUpUserCommand(ExistingUsername, Email, FirstName, LastName, Password, false, null);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

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
        var command = new SignUpUserCommand(Username, ExistingEmail, FirstName, LastName, Password, false, null);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

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
        var command = new SignUpUserCommand(
            Username, 
            Email, 
            FirstName, 
            LastName, 
            Password, 
            false, 
            null,
            new List<Guid> { Guid.CreateVersion7() },
            new List<UserPermissionId>());

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
        }
    }
}