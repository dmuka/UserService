using System.Security.Claims;

namespace Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?
            .FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userId, out var parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User id is unavailable.");
    }
    
    public static string GetUserClaimValue(this ClaimsPrincipal? principal, string claimType)
    {
        var userClaimValue = principal?
            .FindFirst(claim => claim.Type == claimType)?.Value;

        return userClaimValue ?? throw new ApplicationException($"Claim with type {claimType} is unavailable.");
    }
}
