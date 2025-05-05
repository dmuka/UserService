using Domain.Roles;

namespace Application.Roles.GetById;

public sealed record RoleResponse
{
    public required Role Role { get; init; }

    public static RoleResponse Create(Role role)
    {
        var rolesResponse = new RoleResponse { Role = role };
        
        return rolesResponse;
    }
}
