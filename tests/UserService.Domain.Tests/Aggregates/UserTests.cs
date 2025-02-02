using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class UserTests
{
    private const string ValidUsername = "testuser";
    private const string ValidFirstName = "John";
    private const string ValidLastName = "Doe";
    private static readonly PasswordHash ValidPasswordHash = new ("hashedpassword");
    private static readonly Email ValidEmail = new ("test@example.com");
    private static readonly Role ValidRole = new (name: "Admin");

    [Test]
    public void Constructor_Should_Initialize_User_Correctly()
    {
        // Act & Arrange
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Username, Is.EqualTo(ValidUsername));
            Assert.That(user.FirstName, Is.EqualTo(ValidFirstName));
            Assert.That(user.LastName, Is.EqualTo(ValidLastName));
            Assert.That(user.PasswordHash, Is.EqualTo(ValidPasswordHash));
            Assert.That(user.Email, Is.EqualTo(ValidEmail));
            Assert.That(user.Role, Is.EqualTo(ValidRole));
            Assert.That(user.RoleId, Is.EqualTo(ValidRole.Id));
        }
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_Username()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = new User("", ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole));
        Assert.Throws<ArgumentException>(() => _ = new User(null!, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_FirstName()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = new User(ValidUsername, "", ValidLastName, ValidPasswordHash, ValidEmail, ValidRole));
        Assert.Throws<ArgumentException>(() => _ = new User(ValidUsername, null!, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentException_For_Null_Or_Empty_LastName()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentException>(() => _ = new User(ValidUsername, ValidFirstName, "", ValidPasswordHash, ValidEmail, ValidRole));
        Assert.Throws<ArgumentException>(() => _ = new User(ValidUsername, ValidFirstName, null!, ValidPasswordHash, ValidEmail, ValidRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_PasswordHash()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new User(ValidUsername, ValidFirstName, ValidLastName, null!, ValidEmail, ValidRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_Email()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, null!, ValidRole));
    }

    [Test]
    public void Constructor_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Act & Arrange & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, null!));
    }

    [Test]
    public void ChangeEmail_Should_Update_Email_Correctly()
    {
        // Arrange
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);
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
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangeEmail(null!));
    }

    [Test]
    public void ChangePassword_Should_Update_PasswordHash_Correctly()
    {
        // Arrange
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);
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
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangePassword(null!));
    }

    [Test]
    public void ChangeRole_Should_Update_Role_Correctly()
    {
        // Arrange
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);
        var newRole = new Role("User");

        // Act
        user.ChangeRole(newRole);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Role, Is.EqualTo(newRole));
            Assert.That(user.RoleId, Is.EqualTo(newRole.Id));
        }
    }

    [Test]
    public void ChangeRole_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Arrange
        var user = new User(ValidUsername, ValidFirstName, ValidLastName, ValidPasswordHash, ValidEmail, ValidRole);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ChangeRole(null!));
    }
}