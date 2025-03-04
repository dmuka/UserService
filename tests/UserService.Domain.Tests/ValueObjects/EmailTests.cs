using Core;
using Domain.ValueObjects.Emails;

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
        var email = Email.Create(ValidEmail).Value;

        // Assert
        Assert.That(email.Value, Is.EqualTo(ValidEmail));
    }

    [Test]
    public void Constructor_InvalidEmail_ReturnsResultWithProblem()
    {
        // Arrange & Act
        var result = Email.Create(InvalidEmail);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("InvalidEmail"));
            Assert.That(result.Error.Description, Is.EqualTo("The provided email is invalid."));
            Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Problem));
        }
    }

    [Test]
    public void Constructor_EmptyEmail_ReturnResultWithFailure()
    {
        // Arrange & Act
        var result = Email.Create("");

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
    public void Constructor_NullEmail_ReturnResultWithFailure()
    {
        // Arrange & Act
        var result = Email.Create(null!);

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
    public void ImplicitConversion_ToString_ReturnsCorrectValue()
    {
        // Arrange
        var email = Email.Create(ValidEmail).Value;
        var actual = ValidEmail;

        // Act
        string result = email;

        // Assert
        Assert.That(actual, Is.EqualTo(result));
    }

    [Test]
    public void Equals_EqualEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmail).Value;

        // Act
        var result = email1.Equals(email2);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Equals_DifferentEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail);
        var email2 = Email.Create(ValidEmail2);

        // Act
        var result = email1.Equals(email2);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Equals_DifferentCaseEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmailUpperCase).Value;

        // Act
        var result = email1.Equals(email2);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Equals_NullEmail_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail);
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
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmail).Value;

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
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmail2).Value;

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
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmailUpperCase).Value;

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
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmail).Value;

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OperatorEquality_DifferentEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail);
        var email2 = Email.Create(ValidEmail2);

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorEquality_DifferentCaseEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmailUpperCase).Value;

        // Act
        var result = email1 == email2;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OperatorEquality_OneNull_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail).Value;
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
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmail).Value;

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorInequality_DifferentEmails_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail);
        var email2 = Email.Create(ValidEmail2);

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OperatorInequality_DifferentCaseEmails_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail).Value;
        var email2 = Email.Create(ValidEmailUpperCase).Value;

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OperatorInequality_OneNull_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create(ValidEmail).Value;
        Email? email2 = null;

        // Act
        var result = email1 != email2;

        // Assert
        Assert.That(result, Is.True);
    }
}