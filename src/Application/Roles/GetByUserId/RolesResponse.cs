using Domain.Roles;

namespace Application.Roles.GetByUserId;

public sealed record RolesResponse
{
    public required IList<Role> Roles { get; init; }

    public static RolesResponse Create(IList<Role> roles)
    {
        var rolesResponse = new RolesResponse { Roles = roles };
        
        return rolesResponse;
    }
}
