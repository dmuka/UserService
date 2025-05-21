using Application.Abstractions.Authentication;
using Application.Users.SignInByToken;
using Core;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using WebApi.Infrastructure;

namespace WebApi.Middleware;

public class TokenRenewalMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITokenProvider tokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<AuthOptions> authOptions,
        ISender sender)
    {
        var sessionId = context.Request.Cookies[CookiesNames.SessionId];

        if (sessionId is null)
        {
            await next(context);
            return;
        }

        var accessToken = context.Request.Cookies[CookiesNames.AccessToken];
        
        if (string.IsNullOrEmpty(accessToken) ||
            (!string.IsNullOrEmpty(accessToken) && !tokenProvider.ValidateAccessToken(accessToken)))
        {
            Log.Information("Access token is missing or expired, attempting to renew.");
            
            var resultToken = await refreshTokenRepository.GetTokenByIdAsync(Guid.Parse(sessionId));
            
            if (resultToken.IsSuccess)
            {
                var refreshToken = resultToken.Value;
                var command = new SignInUserByTokenCommand(refreshToken.Value, authOptions.Value.RefreshTokenExpirationInDays);
                var result = await sender.Send(command);

                if (!result.IsSuccess) return;
                
                context.Request.Headers.Authorization = $"Bearer {result.Value.AccessToken}";        
                
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = tokenProvider.GetExpirationValue(authOptions.Value.SessionIdCookieExpirationInHours, ExpirationUnits.Hour)
                };

                context.Response.Cookies.Append(CookiesNames.SessionId, sessionId, cookieOptions);

                cookieOptions.Expires =
                    tokenProvider.GetExpirationValue(authOptions.Value.AccessTokenCookieExpirationInMinutes,
                        ExpirationUnits.Minute);
                context.Response.Cookies.Append(CookiesNames.AccessToken, result.Value.AccessToken);
                
                Log.Information("Access token successfully renewed.");
                
                await next(context);
            }
            
            return;
        }

        await next(context);
    }
}