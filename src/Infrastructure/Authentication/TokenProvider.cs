using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Authentication;
using Core;
using Domain.Roles;
using Domain.Users;
using Infrastructure.Options.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(IOptions<AuthOptions> authOptions, IServiceScopeFactory scopeFactory) : ITokenProvider
{
    public async Task<string> CreateAccessTokenAsync(
        User user, 
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        var secretKey = authOptions.Value.Secret;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.AuthenticationMethod, user.IsMfaEnabled ? "mfa" : "pwd")
        ]);

        await using var scope = scopeFactory.CreateAsyncScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var roles = await roleRepository.GetRolesByUserIdAsync(user.Id.Value, cancellationToken);
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Issuer = authOptions.Value.Issuer,
            Audience = authOptions.Value.Audience,
            Expires = GetExpirationValue(authOptions.Value.AccessTokenExpirationInMinutes, ExpirationUnits.Minute, rememberMe)
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
    
    public bool ValidateAccessToken(string? accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(authOptions.Value.Secret);

        try
        {
            var principal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = authOptions.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = authOptions.Value.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public DateTime GetExpirationValue(
        int expirationValue,
        ExpirationUnits expirationUnits,
        bool rememberMe = false)
    {
        if (!rememberMe)
        {
            expirationValue /= 2;
            if (expirationValue == 0) expirationValue = 1;
        }

        var value = expirationUnits switch
        {
            ExpirationUnits.Minute => DateTime.UtcNow.AddMinutes(expirationValue),
            ExpirationUnits.Hour => DateTime.UtcNow.AddHours(expirationValue),
            ExpirationUnits.Day => DateTime.UtcNow.AddDays(expirationValue),
            _ => DateTime.UtcNow.AddMinutes(expirationValue)
        };
        
        return value;
    }
}
