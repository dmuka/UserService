using Core;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class UserTests
{
    private static readonly Guid AuthorizedUserId = Guid.CreateVersion7();
    private static readonly Guid RoleId = Guid.CreateVersion7();
    private static readonly RoleName RoleName = RoleName.Create("Role");
    
    private readonly UserId _userId = new(AuthorizedUserId);
    private readonly ICollection<RoleName> _validRolesNames = new List<RoleName> { RoleName.Create("Role") };
    private readonly ICollection<UserPermissionId> _userPermissionIds = [];
    private readonly ICollection<string> _recoveryCodes = ["recoveryCode"];

    private const string ValidUsername = "testuser";
    private const string ValidFirstName = "John";
    private const string ValidLastName = "Doe";
    private const bool MfaDisabled = false;
    private const string MfaSecret = "MfaSecret";
    
    private static readonly PasswordHash ValidPasswordHash = PasswordHash.Create("hashedpassword").Value;
    private static readonly Email ValidEmail = Email.Create("test@example.com").Value;

    [Test]
    public void Constructor_Should_Initialize_User_Correctly()
    {
        // Act & Arrange
        var user = User.Create(
            _userId.Value,
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Username, Is.EqualTo(ValidUsername));
            Assert.That(user.FirstName, Is.EqualTo(ValidFirstName));
            Assert.That(user.LastName, Is.EqualTo(ValidLastName));
            Assert.That(user.PasswordHash, Is.EqualTo(ValidPasswordHash));
            Assert.That(user.Email, Is.EqualTo(ValidEmail));
            Assert.That(user.RoleNames, Has.Count.EqualTo(1));
        }
    }

    

    [TestCase("")]
    [TestCase(null!)]
    public void Constructor_ShouldReturnResultWithFailure_ForNullOrEmptyUsername(string username)
    {
        // Act & Arrange
        var user = User.Create(
            _userId.Value, 
            "", 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(user.IsFailure, Is.True);
            Assert.That(user.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(user.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(user.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [TestCase("")]
    [TestCase(null!)]
    public void Constructor_ShouldReturnResultWithFailure_ForNullOrEmptyFirstName(string firstName)
    {
        // Act & Arrange
        var user = User.Create(
            _userId.Value,
            ValidUsername,
            firstName,
            ValidLastName,
            ValidPasswordHash,
            ValidEmail,
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(user.IsFailure, Is.True);
            Assert.That(user.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(user.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(user.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [TestCase("")]
    [TestCase(null!)]
    public void Constructor_ShouldReturnResultWithFailure_ForNullOrEmptyLastName(string lastName)
    {
        // Act & Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            lastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(user.IsFailure, Is.True);
            Assert.That(user.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(user.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(user.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [Test]
    public void Constructor_ShouldReturnResultWithFailure_ForNullPasswordHash()
    {
        // Act & Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            null!, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(user.IsFailure, Is.True);
            Assert.That(user.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(user.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(user.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [Test]
    public void Constructor_ShouldReturnResultWithFailure_ForNullEmail()
    {
        // Act & Arrange & Assert
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            null!, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(user.IsFailure, Is.True);
            Assert.That(user.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(user.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(user.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [Test]
    public void Constructor_ShouldReturnResultWithFailure_ForNullRole()
    {
        // Act & Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            null!,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(user.IsFailure, Is.True);
            Assert.That(user.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(user.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(user.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [Test]
    public void ChangeEmail_Should_Update_Email_Correctly()
    {
        // Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;
        var newEmail = Email.Create("newemail@example.com");

        // Act
        user.ChangeEmail(newEmail.Value);

        // Assert
        Assert.That(user.Email, Is.EqualTo(newEmail.Value));
    }

    [Test]
    public void ChangeEmail_ShouldReturnFailureResult_ForNullEmail()
    {
        // Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;
        
        // Act
        var result = user.ChangeEmail(null!);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("General.Null"));
            Assert.That(result.Error.Description, Is.EqualTo("Null value was provided"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Failure));
        }
    }

    [Test]
    public void ChangePassword_Should_Update_PasswordHash_Correctly()
    {
        // Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;
        var newPasswordHash = PasswordHash.Create("newhashedpassword");

        // Act
        user.ChangePassword(newPasswordHash.Value);

        // Assert
        Assert.That(user.PasswordHash, Is.EqualTo(newPasswordHash.Value));
    }

    [Test]
    public void ChangePassword_Should_Throw_ArgumentNullException_For_Null_PasswordHash()
    {
        // Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;
        
        // Act
        var result = user.ChangePassword(null!);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Act & Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("General.Null"));
            Assert.That(result.Error.Description, Is.EqualTo("Null value was provided"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Failure));
        }
    }

    [Test]
    public void RemoveRole_Should_Update_Roles_Correctly()
    {
        // Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;
        user.AddRole(RoleName);

        // Act
        user.RemoveRole(RoleName);
        
        // Assert
        Assert.That(user.RoleNames, Has.Count.EqualTo(1));
    }

    [Test]
    public void RemoveRole_Should_Throw_ArgumentNullException_For_Null_Role()
    {
        // Arrange
        var user = User.Create(
            _userId.Value, 
            ValidUsername, 
            ValidFirstName, 
            ValidLastName, 
            ValidPasswordHash, 
            ValidEmail, 
            _validRolesNames,
            _userPermissionIds,
            _recoveryCodes, 
            MfaDisabled,
            MfaSecret).Value;

        // Act
        var result = user.RemoveRole(null!);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Act & Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("General.Null"));
            Assert.That(result.Error.Description, Is.EqualTo("Null value was provided"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Failure));
        }
    }
}