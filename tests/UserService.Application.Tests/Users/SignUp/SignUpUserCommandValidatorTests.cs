using Application.Users.SignUp;
using FluentValidation.TestHelper;

namespace UserService.Application.Tests.Users.SignUp;

[TestFixture]
public class SignUpUserCommandValidatorTests
{
    private const string Username = "username";
    private const string Email = "email@email.com";
    private const string InvalidEmail = "emailemail.com";
    private const string FirstName = "firstName";
    private const string LastName = "lastName";
    private const string Password = "password";
    private const string ShortPassword = "p";
        
    private SignUpUserCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new SignUpUserCommandValidator();
    }

    [Test]
    public void ShouldHaveError_WhenUsernameIsEmpty()
    {
        // Arrange
        var command = new SignUpUserCommand(
            string.Empty,
            Email,
            FirstName,
            LastName,
            Password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Test]
    public void ShouldHaveError_WhenFirstNameIsEmpty()
    {
        // Arrange
        var command = new SignUpUserCommand(
            Username,
            Email,
            string.Empty,
            LastName,
            Password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Test]
    public void ShouldHaveError_WhenLastNameIsEmpty()
    {
        // Arrange
        var command = new SignUpUserCommand(
            Username,
            Email,
            FirstName,
            string.Empty,
            Password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Test]
    public void ShouldHaveError_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new SignUpUserCommand(
            Username,
            InvalidEmail,
            FirstName,
            LastName,
            Password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Test]
    public void ShouldHaveError_WhenPasswordIsTooShort()
    {
        // Arrange
        var command = new SignUpUserCommand(
            Username,
            Email,
            FirstName,
            LastName,
            ShortPassword);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Test]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new SignUpUserCommand(
            Username,
            Email,
            FirstName,
            LastName,
            Password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}