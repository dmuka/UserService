namespace Application.Abstractions.Email;

public interface ITokenHandler
{
    void StoreToken(string accessToken);

    void StoreSessionId(Guid sessionId);

    void ClearTokens();

    string? GetAccessToken();

    Guid? GetSessionId();

    Task<string?> GetRefreshTokenByUserIdAsync(CancellationToken cancellationToken = default);

    Task<string?> GetRefreshTokenBySessionIdAsync(CancellationToken cancellationToken = default);

    string GetEmailToken(string userId);

    bool ValidatePasswordResetToken(string token, out string? userId);

    bool ValidateEmailToken(string token, out string? userId);
}