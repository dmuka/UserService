using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApi.Middleware;

public class TokenValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Cookies["AccessToken"];
        
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken != null && jwtToken.ValidTo > DateTime.UtcNow)
            {
                var claims = jwtToken.Claims;
                var identity = new ClaimsIdentity(claims, "Bearer");
                var principal = new ClaimsPrincipal(identity);
                
                context.User = principal;
            }
        }

        await next(context);
    }
}