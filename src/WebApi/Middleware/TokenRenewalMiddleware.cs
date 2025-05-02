using Application.Abstractions.Authentication;
using Application.Users.SignInByToken;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;

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
        var sessionId = context.Request.Cookies["SessionId"];
        var accessToken = context.Request.Cookies["AccessToken"];
        
        if (!string.IsNullOrEmpty(sessionId) 
            && !string.IsNullOrEmpty(accessToken) 
            && !tokenProvider.ValidateAccessToken(accessToken))
        {
            Log.Information("Token expired, attempting to renew.");
            
            var refreshToken = await refreshTokenRepository.GetTokenByIdAsync(Guid.Parse(sessionId));
            
            if (refreshToken != null && refreshToken.ExpiresUtc > DateTime.UtcNow)
            {
                var command = new SignInUserByTokenCommand(refreshToken.Value, authOptions.Value.RefreshTokenExpirationInDays);
                var result = await sender.Send(command);

                if (!result.IsSuccess) return;
                
                context.Request.Headers.Authorization = $"Bearer {result.Value.AccessToken}";        
                
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(authOptions.Value.SessionIdCookieExpirationInHours)
                };

                context.Response.Cookies.Append("SessionId", sessionId, cookieOptions);
                
                cookieOptions.Expires = DateTime.UtcNow.AddMinutes(authOptions.Value.AccessTokenCookieExpirationInMinutes);
                context.Response.Cookies.Append("AccessToken", result.Value.AccessToken);
                
                Log.Information("Token successfully renewed.");

                await next(context);
            }
            
            return;
        }

        await next(context);
    }
}