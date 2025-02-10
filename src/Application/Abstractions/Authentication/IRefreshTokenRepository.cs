using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetTokenByUserAsync(User user, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetTokenAsync(string value, CancellationToken cancellationToken = default);
    Task AddTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
}