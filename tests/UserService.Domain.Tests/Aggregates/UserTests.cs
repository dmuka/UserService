using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class UserTests
{
    private static readonly Guid AuthorizedUserId = Guid.CreateVersion7();
    private static readonly Guid RoleId = Guid.CreateVersion7();
    
    private readonly UserId _userId = new(AuthorizedUserId);
    private readonly ICollection<RoleId> _validRolesIds = new List<RoleId> { new (RoleId) };
    private readonly ICollection<UserPermissionId> _userPermissionIds = [];

    private const string ValidUsername = "testuser";
    private const string ValidFirstName = "John";
    private const string ValidLastName = "Doe";
    
    private static readonly PasswordHash ValidPasswordHash = new ("hashedpassword");
    private static readonly Email ValidEmail = new("test@example.com");

    [Test]
    public void Constructor_Should_Initialize_User_Correctly()
    {
        // Act & Arrange
        var user = User.CreateUser(
            _userId.Value,
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Username, Is.EqualTo(ValidUsername));
            Assert.That(user.FirstName, Is.EqualTo(ValidFirstName));
            Assert.That(user.LastName, Is.EqualTo(ValidLastName));
            Assert.That(user.PasswordHash, Is.EqualTo(ValidPasswordHash));
            Assert.That(user.Email, Is.EqualTo(ValidEmail));
            Assert.That(user.RoleIds, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_Username()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = User.CreateUser(
            _userId.Value, 
            "", 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
        Assert.Throws<ArgumentException>(() => _ = User.CreateUser(
            _userId.Value, 
            null!, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_FirstName()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            "", 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
        Assert.Throws<ArgumentException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            null!, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_LastName()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            "", 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
        Assert.Throws<ArgumentException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            null!, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_PasswordHash()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            null!, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_Email()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            null!, 
            _validRolesIds,
            _userPermissionIds));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            null!,
            _userPermissionIds));
    }

    [Test]
    public void ChangeEmail_Should_Update_Email_Correctly()
    {
        // Arrange
        var user = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);
        var newEmail = new Email("newemail@example.com");

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        Assert.That(user.Email, Is.EqualTo(newEmail));
    }

    [Test]
    public void ChangeEmail_Should_Throw_ArgumentNullException_For_Null_Email()
    {
        // Arrange
        var user = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangeEmail(null!));
    }

    [Test]
    public void ChangePassword_Should_Update_PasswordHash_Correctly()
    {
        // Arrange
        var user = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);
        var newPasswordHash = new PasswordHash("newhashedpassword");

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        Assert.That(user.PasswordHash, Is.EqualTo(newPasswordHash));
    }

    [Test]
    public void ChangePassword_Should_Throw_ArgumentNullException_For_Null_PasswordHash()
    {
        // Arrange
        var user = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangePassword(null!));
    }

    [Test]
    public void RemoveRole_Should_Update_Roles_Correctly()
    {
        // Arrange
        var user = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);
        var newRoleId = new RoleId(RoleId);
        user.AddRole(newRoleId);

        // Act
        user.RemoveRole(newRoleId);
        
        // Assert
        Assert.That(user.RoleIds, Has.Count.EqualTo(1));
    }

    [Test]
    public void RemoveRole_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Arrange
        var user = User.CreateUser(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesIds,
            _userPermissionIds);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.RemoveRole(null!));
    }
}