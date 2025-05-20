using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Infrastructure.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
    {
        if (context.User.Identity is not null && !context.User.Identity.IsAuthenticated) return Task.CompletedTask;
        
        var userRoles = context.User
            .FindAll(ClaimTypes.Role).Select(c => c.Value);
        
        if (requirement.AllowedRoles.Any(role => userRoles.Contains(role)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}