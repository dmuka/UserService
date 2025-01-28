using Domain.ValueObjects;

namespace UserService.Domain.Tests.ValueObjects;

[TestFixture]
public class EmailTests
{
    private const string ValidEmail = "test@test.com";
    private const string ValidEmailUpperCase = "TEST@TEST.COM";
    private const string ValidEmail2 = "test2@test.com";
    private const string InvalidEmail = "invalid-email";
    
    [SetUp]
    public void Setup()
    {
        
    }
    
    [Test]
    public void Constructor_ValidEmail_SetsValue()
    {
        // Arrange
        // Act
        var email = new Email(ValidEmail);

        // Assert
        Assert.That(email.Value, Is.EqualTo(ValidEmail));
    }

    [Test]
    public void Constructor_InvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new Email(InvalidEmail));
    }

    [Test]
    public void Constructor_NullEmail_ThrowsArgumentException()
    {
        // Arrange
        string? nullEmail = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new Email(nullEmail!));
    }

    [Test]
    public void Constructor_EmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        const string emptyEmail = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = new Email(emptyEmail));
    }

    [Test]
    public void ImplicitConversion_ToString_ReturnsCorrectValue()
    {
        // Arrange
        var email = new Email(ValidEmail);
        var actual = ValidEmail;

        // Act
        string result = email;

        // Assert
        Assert.That(actual, Is.EqualTo(result));
    }

    [Test]
    public void ExplicitConversion_FromString_CreatesValidEmail()
    {
        // Arrange
        var validEmail = new Email(ValidEmail);
        
        // Act
        var email = (Email)ValidEmail;

        // Assert
        Assert.That(validEmail, Is.EqualTo(email));
    }

    [Test]
    public void ExplicitConversion_FromInvalidString_ThrowsArgumentException()
    {
        // Arrange
        var invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _ = (Email)invalidEmail);
    }

    [Test]
    public void Equals_EqualEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail);

        // Act
        var result = email1.Equals(email2);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Equals_DifferentEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail2);

        // Act
        var result = email1.Equals(email2);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Equals_DifferentCaseEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmailUpperCase);

        // Act
        var result = email1.Equals(email2);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Equals_NullEmail_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        Email? email2 = null;

        // Act
        var result = email1.Equals(email2!);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetHashCode_EqualEmails_ReturnSameHashCode()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail);

        // Act
        var hashCode1 = email1.GetHashCode();
        var hashCode2 = email2.GetHashCode();

        // Assert
        Assert.That(hashCode1, Is.EqualTo(hashCode2));
    }

    [Test]
    public void GetHashCode_DifferentEmails_ReturnDifferentHashCodes()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail2);

        // Act
        var hashCode1 = email1.GetHashCode();
        var hashCode2 = email2.GetHashCode();

        // Assert
        Assert.That(hashCode1, Is.Not.EqualTo(hashCode2));
    }

    [Test]
    public void GetHashCode_DifferentCaseEmails_ReturnSameHashCode()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmailUpperCase);

        // Act
        var hashCode1 = email1.GetHashCode();
        var hashCode2 = email2.GetHashCode();

        // Assert
        Assert.That(hashCode1, Is.EqualTo(hashCode2));
    }

    [Test]
    public void OperatorEquality_EqualEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail);

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OperatorEquality_DifferentEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail2);

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorEquality_DifferentCaseEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmailUpperCase);

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OperatorEquality_OneNull_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        Email? email2 = null;

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorInequality_EqualEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail);

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorInequality_DifferentEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmail2);

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OperatorInequality_DifferentCaseEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        var email2 = new Email(ValidEmailUpperCase);

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorInequality_OneNull_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email(ValidEmail);
        Email? email2 = null;

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.True);
    }
}