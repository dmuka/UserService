using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class UserTests
{
    private const long AuthorizedUserId = 123;
    private const long RoleId = 1;
    
    private readonly UserId _userId = new(AuthorizedUserId);
    private readonly RoleId _roleId = new(RoleId);
    private readonly Role _validRole = new(new RoleId(RoleId), "Admin");

    private const string ValidUsername = "testuser";
    private const string ValidFirstName = "John";
    private const string ValidLastName = "Doe";
    
    private static readonly PasswordHash ValidPasswordHash = new ("hashedpassword");
    private static readonly Email ValidEmail = new("test@example.com");

    [Test]
    public void Constructor_Should_Initialize_User_Correctly()
    {
        // Act & Arrange
        var user = new User(_userId,ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Username, Is.EqualTo(ValidUsername));
            Assert.That(user.FirstName, Is.EqualTo(ValidFirstName));
            Assert.That(user.LastName, Is.EqualTo(ValidLastName));
            Assert.That(user.PasswordHash, Is.EqualTo(ValidPasswordHash));
            Assert.That(user.Email, Is.EqualTo(ValidEmail));
            Assert.That(user.Role, Is.EqualTo(_validRole));
        }
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_Username()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = new User(_userId, "", ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole));
        Assert.Throws<ArgumentException>(() => _ = new User(_userId, null!, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_FirstName()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = new User(_userId, ValidUsername, "", ValidLastName, ValidPasswordHash, ValidEmail, _validRole));
        Assert.Throws<ArgumentException>(() => _ = new User(_userId, ValidUsername, null!, ValidLastName, ValidPasswordHash, ValidEmail, _validRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_LastName()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = new User(_userId, ValidUsername, ValidFirstName, "", ValidPasswordHash, ValidEmail, _validRole));
        Assert.Throws<ArgumentException>(() => _ = new User(_userId, ValidUsername, ValidFirstName, null!, ValidPasswordHash, ValidEmail, _validRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_PasswordHash()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, null!, ValidEmail, _validRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_Email()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, null!, _validRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, null!));
    }

    [Test]
    public void ChangeEmail_Should_Update_Email_Correctly()
    {
        // Arrange
        var user = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);
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
        var user = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangeEmail(null!));
    }

    [Test]
    public void ChangePassword_Should_Update_PasswordHash_Correctly()
    {
        // Arrange
        var user = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);
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
        var user = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangePassword(null!));
    }

    [Test]
    public void ChangeRole_Should_Update_Role_Correctly()
    {
        // Arrange
        var user = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);
        var newRole = new Role(_roleId,"User");

        // Act
        user.ChangeRole(newRole);
        
        // Assert
        Assert.That(user.Role, Is.EqualTo(newRole));
    }

    [Test]
    public void ChangeRole_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Arrange
        var user = new User(_userId, ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, _validRole);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangeRole(null!));
    }
}