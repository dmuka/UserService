namespace Application.Abstractions.Authentication;

/// <summary>
/// Represents the context of the currently authenticated user.
/// </summary>
public interface IUserContext
{
    Guid UserId { get; }
    string UserName { get; }
    string Email { get; }
    string UserRole { get; }
    bool IsAuthenticated { get; }
    string AuthMethod { get; }
}
