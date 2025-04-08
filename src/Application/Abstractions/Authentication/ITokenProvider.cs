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
    Task<string> CreateAccessTokenAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <returns>A new refresh token as a string.</returns>
    string CreateRefreshToken();
    
    /// <summary>
    /// Validates the provided access token.
    /// </summary>
    /// <param name="accessToken">The JWT access token to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    /// <remarks>
    /// This method checks the token's signature and expiration to ensure it is valid and trustworthy.
    /// </remarks>
    bool ValidateAccessToken(string? accessToken);
}