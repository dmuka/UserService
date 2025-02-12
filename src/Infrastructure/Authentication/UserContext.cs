using System.Security.Claims;
using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
    
namespace Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private const string ContextUnavailable = "User context is unavailable.";
    
    public Guid UserId =>
        httpContextAccessor.HttpContext?.User.GetUserId() ??
        throw new ApplicationException(ContextUnavailable);
    
    public string UserName =>
        httpContextAccessor.HttpContext?.User.GetUserClaimValue(ClaimTypes.Name) ??
        throw new ApplicationException(ContextUnavailable);
    
    public string Email =>
        httpContextAccessor.HttpContext?.User.GetUserClaimValue(ClaimTypes.Email) ??
        throw new ApplicationException(ContextUnavailable);
    
    public string UserRole =>
        httpContextAccessor.HttpContext?.User.GetUserClaimValue(ClaimTypes.Role) ??
        throw new ApplicationException(ContextUnavailable);

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ??
        throw new ApplicationException("User context is unavailable");
}
