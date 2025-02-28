using Domain.Roles;

namespace Application.Roles.GetByUserId;

public sealed record RoleResponse
{
    public required Role Role { get; init; }

    public static RoleResponse Create(Role role)
    {
        var rolesResponse = new RoleResponse { Role = role };
        
        return rolesResponse;
    }
}
