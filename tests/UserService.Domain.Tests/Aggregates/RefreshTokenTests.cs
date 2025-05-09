using Core;
using Domain.RefreshTokens;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class RefreshTokenTests
{
    private static readonly Guid Id = Guid.CreateVersion7();
    private const string SampleTokenValue = "Sample Token";
    
    private readonly User _user = User.Create(
        Id, 
        "username", 
        "firstName", 
        "lastName", 
        "hash", 
        "email@email.com",
        new List<RoleId> { new (Guid.CreateVersion7()) },
        new List<UserPermissionId>(),
        ["recoveryCode"], 
        false,
        "MfaSecret").Value;
    
    [Test]
    public void Create_ShouldInitializeProperties()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var result = RefreshToken.Create(Id, SampleTokenValue, expiresUtc, _user.Id);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Value.Id, Is.EqualTo(new RefreshTokenId(Id)));
            Assert.That(result.Value.Value, Is.EqualTo(SampleTokenValue));
            Assert.That(result.Value.ExpiresUtc, Is.EqualTo(expiresUtc));
            Assert.That(result.Value.UserId, Is.EqualTo(_user.Id));
        }
    }

    [Test]
    public void ChangeValue_ShouldUpdateValue()
    {
        // Arrange
        var refreshToken = CreateSampleRefreshToken();
        const string newValue = "newTokenValue";

        // Act
        refreshToken.ChangeValue(newValue);

        // Assert
        Assert.That(refreshToken.Value, Is.EqualTo(newValue));
    }

    [Test]
    public void ChangeExpireDate_ShouldUpdateExpireDate()
    {
        // Arrange
        var refreshToken = CreateSampleRefreshToken();
        var newExpireDate = DateTime.UtcNow.AddDays(2);

        // Act
        refreshToken.ChangeExpireDate(newExpireDate);

        // Assert
        Assert.That(refreshToken.ExpiresUtc, Is.EqualTo(newExpireDate));
    }

    [TestCase(null)]
    [TestCase("")]
    public void Create_ShouldReturnResultWithFailure_WhenValueIsNullOrEmpty(string? newValue)
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var result = RefreshToken.Create(Id, newValue, expiresUtc, _user.Id);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(result.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [Test]
    public void Create_ShouldReturnResultWithFailure_WhenExpireDateIsPast()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = RefreshToken.Create(Id, SampleTokenValue, expiresUtc, _user.Id);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(result.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    [Test]
    public void Create_ShouldReturnResultWithFailure_WhenUserIdIsNull()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var result = RefreshToken.Create(Id, SampleTokenValue, expiresUtc, null);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("Validation.General"));
            Assert.That(result.Error.Description, Is.EqualTo("One or more validation errors occurred"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Validation));
        }
    }

    private RefreshToken CreateSampleRefreshToken()
    {
        var expiresUtc = DateTime.UtcNow.AddDays(1);
        return RefreshToken.Create(Id, SampleTokenValue, expiresUtc, _user.Id).Value;
    }
}