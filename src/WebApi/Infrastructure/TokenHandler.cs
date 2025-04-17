using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Infrastructure.Options.Authentication;
using Microsoft.Extensions.Options;

namespace WebApi.Infrastructure;

public class TokenHandler(
    IUserContext userContext, 
    IHttpContextAccessor httpContextAccessor,
    IRefreshTokenRepository refreshTokenRepository,
    IHttpClientFactory httpClientFactory,
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
            Expires = DateTime.UtcNow.AddMinutes(authOptions.Value.ExpirationInMinutes)
        };

        httpContextAccessor.HttpContext.Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
    }

    public void ClearTokens()
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete("AccessToken");
    }

    public string? GetAccessToken()
    {
        return httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
    }

    public async Task<string> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var refreshToken = await refreshTokenRepository.GetTokenByUserIdAsync(userId, cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresUtc < DateTime.UtcNow) return null;
        
        return refreshToken.Value;
    }

    public async Task<bool> RefreshTokens(CancellationToken cancellationToken = default)
    {
        if (httpContextAccessor.HttpContext is null) return false;
        
        var refreshToken = await GetRefreshTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(refreshToken)) return false;
        
        var currentRequest = httpContextAccessor.HttpContext.Request;
        var baseUri = $"{currentRequest.Scheme}://{currentRequest.Host}";
        var requestUri = new Uri(new Uri(baseUri), "api/users/signinbytoken");
        
        var client = httpClientFactory.CreateClient();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { refreshToken }),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokens = JsonSerializer.Deserialize<TokenResponse>(content);

            if (tokens is null) return false;

            StoreToken(tokens.AccessToken);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error (exception: {exception}, message: {message}) occurred while trying to refresh tokens for user id: {UserId}.", ex, ex.Message, userContext.UserId);

            return false;
        }
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}