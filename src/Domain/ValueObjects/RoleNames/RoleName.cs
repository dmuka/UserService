using Core;

namespace Domain.ValueObjects.RoleNames;

public sealed class RoleName : ValueObject
{
    /// <summary>
    /// Gets the role name value.
    /// </summary>
    public string Value { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleName"/> class.
    /// </summary>
    /// <param name="value">The role name value.</param>
    private RoleName(string value) => Value = value;
    
    /// <summary>
    /// Creates a new RoleName instance if the provided value is valid.
    /// </summary>
    /// <param name="value">The role name value to validate and store.</param>
    /// <returns>A Result containing the RoleName instance or a validation error.</returns>
    public static Result<RoleName> Create(string value)
    {
        if (value is null) return Result.Failure<RoleName>(Error.NullValue);

        if (string.IsNullOrWhiteSpace(value)) return Result.Failure<RoleName>(Error.EmptyValue);

        return Result.Success(new RoleName(value));
    }

    /// <summary>
    /// Implicitly converts a <see cref="RoleName"/> to a string.
    /// </summary>
    /// <param name="roleName">The role name to convert.</param>
    public static implicit operator string(RoleName roleName) => roleName.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Result{RoleName}" /> to a <see cref="RoleName" />.
    /// </summary>
    /// <param name="roleNameResult">The result with the role name to convert.</param>
    public static implicit operator RoleName(Result<RoleName> roleNameResult) => roleNameResult.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="RoleName"/>.
    /// </summary>
    /// <param name="roleName">The string to convert.</param>
    public static explicit operator RoleName(string roleName) => new (roleName);

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
    public override bool Equals(object obj)
    {
        if (obj is null || GetType() != obj.GetType())
            return false;

        var other = (RoleName)obj;
        return Value == other.Value;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
}