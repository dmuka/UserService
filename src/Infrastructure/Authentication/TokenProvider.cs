using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Authentication;
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
    public async Task<string> CreateAccessTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var secretKey = authOptions.Value.Secret;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        ]);

        await using var scope = scopeFactory.CreateAsyncScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var roles = await roleRepository.GetRolesByUserIdAsync(user.Id.Value, cancellationToken);
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(authOptions.Value.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = authOptions.Value.Issuer,
            Audience = authOptions.Value.Audience
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
