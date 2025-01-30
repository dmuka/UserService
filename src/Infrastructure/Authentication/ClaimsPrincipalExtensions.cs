using System.Security.Claims;

namespace Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static ulong GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?
            .FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        return ulong.TryParse(userId, out var parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User id is unavailable");
    }
}
