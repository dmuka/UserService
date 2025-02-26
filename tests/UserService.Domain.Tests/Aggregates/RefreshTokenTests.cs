using Domain.RefreshTokens;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class RefreshTokenTests
{
    private static readonly Guid Id = Guid.CreateVersion7();
    private const string SampleTokenValue = "Sample Token";
    private readonly User _user = User.CreateUser(
        Id, 
        "username", 
        "firstName", 
        "lastName", 
        new PasswordHash("hash"), 
        new Email("email@email.com"), 
        new List<Role>());
    
    [Test]
    public void Create_ShouldInitializeProperties()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var refreshToken = RefreshToken.Create(Id, SampleTokenValue, expiresUtc, _user);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(refreshToken.Id, Is.EqualTo(new RefreshTokenId(Id)));
            Assert.That(refreshToken.Value, Is.EqualTo(SampleTokenValue));
            Assert.That(refreshToken.ExpiresUtc, Is.EqualTo(expiresUtc));
            Assert.That(refreshToken.User, Is.EqualTo(_user));
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

    [Test]
    public void Create_ShouldThrowException_WhenValueIsNullOrEmpty()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshToken.Create(Id, null, expiresUtc, _user));
        Assert.Throws<ArgumentException>(() => RefreshToken.Create(Id, "", expiresUtc, _user));
    }

    [Test]
    public void Create_ShouldThrowException_WhenExpireDateIsPast()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshToken.Create(Id, SampleTokenValue, expiresUtc, _user));
    }

    [Test]
    public void Create_ShouldThrowException_WhenUserIsNull()
    {
        // Arrange
        var expiresUtc = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RefreshToken.Create(Id, SampleTokenValue, expiresUtc, null));
    }

    private RefreshToken CreateSampleRefreshToken()
    {
        var expiresUtc = DateTime.UtcNow.AddDays(1);
        return RefreshToken.Create(Id, SampleTokenValue, expiresUtc, _user);
    }
}