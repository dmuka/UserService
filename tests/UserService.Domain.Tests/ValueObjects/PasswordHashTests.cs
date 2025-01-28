using Domain.ValueObjects;

namespace UserService.Domain.Tests.ValueObjects;

[TestFixture]
public class PasswordHashTests
{
    private const string ValidHash = "12345678";
    private const string ValidHash2 = "123456789";
    
    [Test]
    public void Constructor_ValidHash_ShouldSetPropertyValue()
    {
        // Arrange
        // Act
        var passwordHash = _ = new PasswordHash(ValidHash);

        // Assert
        Assert.That(passwordHash.Value, Is.EqualTo(ValidHash));
    }

    [Test]
    public void Constructor_EmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        const string hash = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new PasswordHash(hash));
    }

    [Test]
    public void Constructor_WhiteSpaceString_ShouldThrowArgumentException()
    {
        // Arrange
        const string hash = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new PasswordHash(hash));
    }

    [Test]
    public void Constructor_NullString_ShouldThrowArgumentException()
    {
        // Arrange
        string? hash = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new PasswordHash(hash!));
    }

    [Test]
    public void ImplicitConversion_PasswordHashToString_ShouldReturnCorrectValue()
    {
        // Arrange
        var passwordHash = _ = new PasswordHash(ValidHash);

        // Act
        string result = passwordHash;

        // Assert
        Assert.That(result, Is.EqualTo(ValidHash));
    }

    [Test]
    public void ExplicitConversion_StringToPasswordHash_ShouldCreateCorrectInstance()
    {
        // Arrange
        // Act
        var passwordHash = (PasswordHash)ValidHash;

        // Assert
        Assert.That(passwordHash, Is.Not.Null);
        Assert.That(passwordHash.Value, Is.EqualTo(ValidHash));
    }

    [Test]
    public void Equals_SameHash_ShouldReturnTrue()
    {
        // Arrange
        var hash1 = _ = new PasswordHash(ValidHash);
        var hash2 = _ = new PasswordHash(ValidHash);

        // Act
        var result = hash1.Equals(hash2);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Equals_DifferentHash_ShouldReturnFalse()
    {
        // Arrange
        var hash1 = _ = new PasswordHash(ValidHash);
        var hash2 = _ = new PasswordHash(ValidHash2);

        // Act
        var result = hash1.Equals(hash2);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetHashCode_SameHash_ShouldReturnSameValue()
    {
        // Arrange
        var hash1 = _ = new PasswordHash(ValidHash);
        var hash2 = _ = new PasswordHash(ValidHash);

        // Act
        var hashCode1 = hash1.GetHashCode();
        var hashCode2 = hash2.GetHashCode();

        // Assert
        Assert.That(hashCode1, Is.EqualTo(hashCode2));
    }

    [Test]
    public void GetHashCode_DifferentHash_ShouldReturnDifferentValues()
    {
        // Arrange
        var hash1 = _ = new PasswordHash(ValidHash);
        var hash2 = _ = new PasswordHash(ValidHash2);

        // Act
        var hashCode1 = hash1.GetHashCode();
        var hashCode2 = hash2.GetHashCode();

        // Assert
        Assert.That(hashCode1, Is.Not.EqualTo(hashCode2));
    }

    [Test]
    public void ToString_ShouldReturnHashValue()
    {
        // Arrange
        var passwordHash = _ = new PasswordHash(ValidHash);

        // Act
        var result = passwordHash.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(ValidHash));
    }
}