namespace Application.Abstractions.Authentication;

/// <summary>
/// Represents the context of the currently authenticated user.
/// </summary>
public interface IUserContext
{
    long UserId { get; }
}
