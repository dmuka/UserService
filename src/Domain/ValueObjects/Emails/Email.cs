using System.Net.Mail;
using Core;

namespace Domain.ValueObjects.Emails;

/// <summary>
/// Represents an email address as a value object.
/// </summary>
public sealed class Email : ValueObject
{
    private bool Equals(Email other)
    {
        return base.Equals(other) && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Email other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Value);
    }

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    /// <param name="value">The email address to validate and store.</param>
    private Email(string value) => Value = value.ToLowerInvariant();
    
    /// <summary>
    /// Creates a new Email instance if the provided value is valid.
    /// </summary>
    /// <param name="value">The email address to validate and store.</param>
    /// <returns>A Result containing the Email instance or a validation error.</returns>
    public static Result<Email> Create(string value)
    {
        if (value is null)
        {
            return Result.Failure<Email>(Error.NullValue);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(Error.EmptyValue);
        }

        return IsValid(value) 
            ? Result.Success(new Email(value)) 
            : Result.Failure<Email>(EmailErrors.InvalidEmail);
    }

    /// <summary>
    /// Validates the given email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email address is valid, otherwise, false.</returns>
    private static bool IsValid(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            
            return mailAddress.Address == email;
        }
        catch 
        {
            return false;
        }
    }

    /// <summary>
    /// Implicitly converts an <see cref="Email"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="email">The email address to convert.</param>
    /// <returns>The email address as a string.</returns>
    public static implicit operator string(Email email) => email.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Result{Email}" /> to a <see cref="Email" />.
    /// </summary>
    /// <param name="emailResult">The result with email to convert.</param>
    public static implicit operator Email(Result<Email> emailResult) => emailResult.Value;

    /// <summary>
    /// Checks if two <see cref="Email"/> objects are equal.
    /// </summary>
    /// <param name="left">The first email address to compare.</param>
    /// <param name="right">The second email address to compare.</param>
    /// <returns>True if both email addresses are equal; otherwise, false.</returns>
    public static bool operator ==(Email? left, Email? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two <see cref="Email"/> objects are not equal.
    /// </summary>
    /// <param name="left">The first email address to compare.</param>
    /// <param name="right">The second email address to compare.</param>
    /// <returns>True if the email addresses are not equal; otherwise, false.</returns>
    public static bool operator !=(Email? left, Email? right) => !(left == right);
    
    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => Value;
}