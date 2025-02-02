using System.Security.Claims;

namespace Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?
            .FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(userId, out var parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User id is unavailable");
    }
}
