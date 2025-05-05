using Application.Abstractions.Authentication;
using Core;
using Infrastructure.Options.Authentication;
using Microsoft.Extensions.Options;

namespace WebApi.Infrastructure;

public class TokenHandler(
    IUserContext userContext, 
    IHttpContextAccessor httpContextAccessor,
    ITokenProvider tokenProvider,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<AuthOptions> authOptions,
    ILogger<TokenHandler> logger)
{
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
}