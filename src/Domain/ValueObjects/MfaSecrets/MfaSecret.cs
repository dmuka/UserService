using Core;

namespace Domain.ValueObjects.MfaSecrets;

public sealed class MfaSecret : ValueObject
{
    /// <summary>
    /// Gets the MFA secret value.
    /// </summary>
    public string Value { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MfaSecret"/> class.
    /// </summary>
    /// <param name="value">The password hash value.</param>
    private MfaSecret(string value) => Value = value;
    
    /// <summary>
    /// Creates a new MFA secret instance if the provided value is valid.
    /// </summary>
    /// <param name="value">The MFA secret value to validate and store.</param>
    /// <returns>A Result containing the MFA secret instance or a validation error.</returns>
    public static Result<MfaSecret> Create(string value)
    {
        if (value is null) return Result.Failure<MfaSecret>(Error.NullValue);

        return string.IsNullOrWhiteSpace(value) 
            ? Result.Failure<MfaSecret>(Error.EmptyValue) 
            : Result.Success(new MfaSecret(value));
    }

    /// <summary>
    /// Implicitly converts a <see cref="MfaSecret"/> to a string.
    /// </summary>
    /// <param name="mfaSecret">The password hash to convert.</param>
    public static implicit operator string(MfaSecret mfaSecret) => mfaSecret.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Result{MfaSecret}" /> to a <see cref="MfaSecret" />.
    /// </summary>
    /// <param name="mfaSecretResult">The result with MFA secret to convert.</param>
    public static implicit operator MfaSecret(Result<MfaSecret> mfaSecretResult) => mfaSecretResult.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="MfaSecret"/>.
    /// </summary>
    /// <param name="mfaSecret">The string to convert.</param>
    public static explicit operator MfaSecret(string mfaSecret) => new (mfaSecret);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
            return false;

        var other = (MfaSecret)obj;
        return Value == other.Value;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
}