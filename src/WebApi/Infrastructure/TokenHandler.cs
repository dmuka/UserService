using Application.Abstractions.Authentication;
using Core;
using Infrastructure.Options.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace WebApi.Infrastructure;

public class TokenHandler(
    IUserContext userContext, 
    IHttpContextAccessor httpContextAccessor,
    ITokenProvider tokenProvider,
    IRefreshTokenRepository refreshTokenRepository,
    IDataProtectionProvider provider,
    IOptions<AuthOptions> authOptions)
{
    private readonly IDataProtector _protector = provider.CreateProtector("PasswordReset");
    
    public void StoreToken(string accessToken)
    {
        if (httpContextAccessor.HttpContext is null) return;
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenProvider.GetExpirationValue(authOptions.Value.AccessTokenCookieExpirationInMinutes, ExpirationUnits.Minute)
        };

        httpContextAccessor.HttpContext.Response.Cookies.Append(CookiesNames.AccessToken, accessToken, cookieOptions);
    }
    
    public void StoreSessionId(Guid sessionId)
    {
        if (httpContextAccessor.HttpContext is null) return;
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenProvider.GetExpirationValue(authOptions.Value.SessionIdCookieExpirationInHours, ExpirationUnits.Hour)
        };

        httpContextAccessor.HttpContext.Response.Cookies.Append(CookiesNames.SessionId, sessionId.ToString(), cookieOptions);
    }

    public void ClearTokens()
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookiesNames.AccessToken);
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookiesNames.SessionId);
    }

    public string? GetAccessToken()
    {
        return httpContextAccessor.HttpContext?.Request.Cookies[CookiesNames.AccessToken];
    }

    public Guid? GetSessionId()
    {
        var sessionId = httpContextAccessor.HttpContext?.Request.Cookies[CookiesNames.SessionId];

        return sessionId is null ? null : Guid.Parse(sessionId);
    }

    public async Task<string?> GetRefreshTokenByUserIdAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var refreshToken = await refreshTokenRepository.GetTokenByUserIdAsync(userId, cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresUtc < DateTime.UtcNow) return null;
        
        return refreshToken.Value;
    }

    public async Task<string?> GetRefreshTokenBySessionIdAsync(CancellationToken cancellationToken = default)
    {
        var sessionId = GetSessionId();
        
        if (sessionId is null) return null;
        
        var refreshToken = await refreshTokenRepository.GetTokenByIdAsync(sessionId.Value, cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresUtc < DateTime.UtcNow) return null;
        
        return refreshToken.Value;
    }
    
    public string GetPasswordResetToken(string userId)
    {
        var data = $"{userId}:{DateTime.UtcNow.Ticks}";
        return _protector.Protect(data);
    }

    public bool ValidatePasswordResetToken(string token, out string? userId)
    {
        try
        {
            var parts = _protector.Unprotect(token).Split(':');
            
            userId = parts[0];
            if (!long.TryParse(parts[1], out var timestamp)) return false;
            var tokenAge = DateTime.UtcNow.Ticks - timestamp;
            
            return tokenAge < TimeSpan.FromMinutes(authOptions.Value.ResetPasswordTokenExpirationInMinutes).Ticks;
        }
        catch
        {
            userId = null;
            
            return false;
        }
    }
}