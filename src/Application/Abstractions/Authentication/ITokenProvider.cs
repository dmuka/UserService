using Core;
using Domain.Users;

namespace Application.Abstractions.Authentication;

/// <summary>
/// Provides methods for creating access and refresh tokens for authentication.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Asynchronously creates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the access token is being created.</param>
    /// <param name="rememberMe">A boolean indicating whether the token should have an extended expiration time.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated access token as a string.</returns>
    Task<string> CreateAccessTokenAsync(User user, bool rememberMe, CancellationToken cancellationToken);

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
    
    /// <summary>
    /// Calculates the expiration date and time based on the specified expiration value and units.
    /// </summary>
    /// <param name="expirationValue">The numeric value representing the duration of expiration.</param>
    /// <param name="expirationUnits">The units in which the expiration value is measured (e.g., minutes, hours, days).</param>
    /// <param name="rememberMe">A boolean indicating whether the expiration should be extended for "remember me" functionality. Defaults to false.</param>
    /// <returns>A <see cref="DateTime"/> representing the calculated expiration date and time.</returns>
    public DateTime GetExpirationValue(int expirationValue, ExpirationUnits expirationUnits, bool rememberMe = false);
}