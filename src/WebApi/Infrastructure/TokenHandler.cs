using System.Text;
using System.Text.Json;

namespace WebApi.Infrastructure;

public class TokenHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
{
    public void StoreTokens(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        httpContextAccessor.HttpContext.Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
        httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
    }

    public string GetAccessToken()
    {
        return httpContextAccessor.HttpContext.Request.Cookies["AccessToken"];
    }

    public string GetRefreshToken()
    {
        return httpContextAccessor.HttpContext.Request.Cookies["RefreshToken"];
    }

    public async Task<bool> RefreshTokens()
    {
        var refreshToken = GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken)) return false;
        
        var currentRequest = httpContextAccessor.HttpContext.Request;
        var baseUri = $"{currentRequest.Scheme}://{currentRequest.Host}";
        var requestUri = new Uri(new Uri(baseUri), "users/signinbytoken");
        
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = new StringContent(JsonSerializer.Serialize(new { refreshToken }), 
                         Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;
        
        var content = await response.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<TokenResponse>(content);
            
        StoreTokens(tokens.AccessToken, tokens.RefreshToken);
        
        return true;
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}