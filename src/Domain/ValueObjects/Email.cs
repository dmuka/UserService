using System.Net.Mail;

namespace Domain.ValueObjects;

/// <summary>
/// Represents an email address as a value object.
/// </summary>
public class Email : ValueObject
{
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
    /// <exception cref="ArgumentException">Thrown when the provided email address is invalid.</exception>
    public Email(string value)
    {
        if (!IsValid(value)) throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

        Value = value.ToLowerInvariant();
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
        catch (ArgumentNullException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (FormatException)
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
    /// Explicitly converts a <see cref="string"/> to an <see cref="Email"/>.
    /// </summary>
    /// <param name="email">The string to convert.</param>
    /// <returns>The email address as an <see cref="Email"/> object.</returns>
    public static explicit operator Email(string email) => new (email);

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