using Domain.RefreshTokens;
using Domain.Users;

namespace Application.Abstractions.Authentication;

/// <summary>
/// Defines methods for managing refresh tokens in the authentication process.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Retrieves a refresh token associated with a specific user.
    /// </summary>
    /// <param name="user">The user whose refresh token is to be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The refresh token associated with the user, or <c>null</c> if not found.</returns>
    Task<RefreshToken?> GetTokenByUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a refresh token by its value.
    /// </summary>
    /// <param name="value">The value of the refresh token to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The refresh token with the specified value, or <c>null</c> if not found.</returns>
    Task<RefreshToken?> GetTokenAsync(string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new refresh token to the repository.
    /// </summary>
    /// <param name="token">The refresh token to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task AddTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing refresh token in the repository.
    /// </summary>
    /// <param name="token">The refresh token to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
}