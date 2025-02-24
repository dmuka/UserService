using Infrastructure.Authentication;

namespace UserService.Infrastructure.Tests.Authentication;

[TestFixture]
public class PasswordHasherTests
{
    private const string Password = "TestPassword123";
        
    private PasswordHasher _passwordHasher;

    [SetUp]
    public void SetUp()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Test]
    public void GetHash_ReturnsNonEmptyHash()
    {
        // Arrange
        // Act
        var hash = _passwordHasher.GetHash(Password);

        // Assert
        Assert.That(hash, Is.Not.Empty);
    }

    [Test]
    public void CheckPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _passwordHasher.GetHash(Password);

        // Act
        var result = _passwordHasher.CheckPassword(Password, hash);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CheckPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        const string wrongPassword = "WrongPassword123";
        var hash = _passwordHasher.GetHash(Password);

        // Act
        var result = _passwordHasher.CheckPassword(wrongPassword, hash);

        // Assert
        Assert.That(result, Is.False);
    }
}