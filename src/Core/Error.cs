namespace Core;

/// <summary>
/// Represents an error with a code, description, and type.
/// </summary>
/// <param name="Code">The unique code identifying the error.</param>
/// <param name="Description">A human-readable description of the error.</param>
/// <param name="Type">The type of the error, indicating its category.</param>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>
    /// Represents a default error with no code or description, categorized as a failure.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Represents an error indicating that a null value was provided.
    /// </summary>
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);
    
    /// <summary>
    /// Represents an error indicating that a empty value was provided.
    /// </summary>
    public static readonly Error EmptyValue = new(
        "General.Empty",
        "Empty value was provided",
        ErrorType.Failure);

    /// <summary>
    /// Creates an error categorized as a failure.
    /// </summary>
    /// <param name="code">The unique code identifying the failure.</param>
    /// <param name="description">A description of the failure.</param>
    /// <returns>An <see cref="Error"/> instance representing the failure.</returns>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates an error categorized as not found.
    /// </summary>
    /// <param name="code">The unique code identifying the not found error.</param>
    /// <param name="description">A description of the not found error.</param>
    /// <returns>An <see cref="Error"/> instance representing the not found error.</returns>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates an error categorized as a problem.
    /// </summary>
    /// <param name="code">The unique code identifying the problem.</param>
    /// <param name="description">A description of the problem.</param>
    /// <returns>An <see cref="Error"/> instance representing the problem.</returns>
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    /// <summary>
    /// Creates an error categorized as a conflict.
    /// </summary>
    /// <param name="code">The unique code identifying the conflict.</param>
    /// <param name="description">A description of the conflict.</param>
    /// <returns>An <see cref="Error"/> instance representing the conflict.</returns>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}