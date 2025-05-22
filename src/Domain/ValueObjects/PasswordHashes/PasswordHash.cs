using Core;

namespace Domain.ValueObjects.PasswordHashes;

public sealed class PasswordHash : ValueObject
{
    /// <summary>
    /// Gets the password hash value.
    /// </summary>
    public string Value { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHash"/> class.
    /// </summary>
    /// <param name="value">The password hash value.</param>
    private PasswordHash(string value) => Value = value;
    
    /// <summary>
    /// Creates a new PasswordHash instance if the provided value is valid.
    /// </summary>
    /// <param name="value">The password hash value to validate and store.</param>
    /// <returns>A Result containing the PasswordHash instance or a validation error.</returns>
    public static Result<PasswordHash> Create(string value)
    {
        if (value is null) return Result.Failure<PasswordHash>(Error.NullValue);

        if (string.IsNullOrWhiteSpace(value)) return Result.Failure<PasswordHash>(Error.EmptyValue);

        return Result.Success(new PasswordHash(value));
    }

    /// <summary>
    /// Implicitly converts a <see cref="PasswordHash"/> to a string.
    /// </summary>
    /// <param name="passwordHash">The password hash to convert.</param>
    public static implicit operator string(PasswordHash passwordHash) => passwordHash.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Result{PasswordHash}" /> to a <see cref="PasswordHash" />.
    /// </summary>
    /// <param name="passwordHashResult">The result with password hash to convert.</param>
    public static implicit operator PasswordHash(Result<PasswordHash> passwordHashResult) => passwordHashResult.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="PasswordHash"/>.
    /// </summary>
    /// <param name="passwordHash">The string to convert.</param>
    public static explicit operator PasswordHash(string passwordHash) => new (passwordHash);

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

        var other = (PasswordHash)obj;
        return Value == other.Value;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
}