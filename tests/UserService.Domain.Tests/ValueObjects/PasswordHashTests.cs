using Core;
using Domain.ValueObjects.PasswordHashes;

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
        var passwordHash = _ = PasswordHash.Create(ValidHash).Value;

        // Assert
        Assert.That(passwordHash.Value, Is.EqualTo(ValidHash));
    }

    [Test]
    public void Constructor_EmptyHash_ReturnResultWithFailure()
    {
        // Arrange & Act
        var result = PasswordHash.Create("");

        // Assert
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("General.Empty"));
            Assert.That(result.Error.Description, Is.EqualTo("Empty value was provided"));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Failure));
        }
    }

    [Test]
    public void Constructor_NullHash_ReturnResultWithFailure()
    {
        // Arrange & Act
        var result = PasswordHash.Create(null!);

        // Assert
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
    public void ImplicitConversion_PasswordHashToString_ShouldReturnCorrectValue()
    {
        // Arrange
        var passwordHash = _ = PasswordHash.Create(ValidHash).Value;

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
        var hash1 = _ = PasswordHash.Create(ValidHash).Value;
        var hash2 = _ = PasswordHash.Create(ValidHash).Value;

        // Act
        var result = hash1.Equals(hash2);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Equals_DifferentHash_ShouldReturnFalse()
    {
        // Arrange
        var hash1 = _ = PasswordHash.Create(ValidHash).Value;
        var hash2 = _ = PasswordHash.Create(ValidHash2).Value;

        // Act
        var result = hash1.Equals(hash2);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetHashCode_SameHash_ShouldReturnSameValue()
    {
        // Arrange
        var hash1 = _ = PasswordHash.Create(ValidHash).Value;
        var hash2 = _ = PasswordHash.Create(ValidHash).Value;

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
        var hash1 = _ = PasswordHash.Create(ValidHash).Value;
        var hash2 = _ = PasswordHash.Create(ValidHash2).Value;

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
        var passwordHash = _ = PasswordHash.Create(ValidHash).Value;

        // Act
        var result = passwordHash.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(ValidHash));
    }
}