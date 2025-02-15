using Domain.Users;

namespace Application.Abstractions.Authentication;

/// <summary>
/// Provides methods for creating access and refresh tokens for authentication.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Creates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the access token is created.</param>
    /// <returns>A JWT access token as a string.</returns>
    string CreateAccessToken(User user);

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <returns>A new refresh token as a string.</returns>
    string CreateRefreshToken();
}