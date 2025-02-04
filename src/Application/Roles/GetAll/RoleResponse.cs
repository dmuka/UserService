using Domain.Roles;

namespace Application.Roles.GetAll;

public sealed record RoleResponse
{
    public required Guid Id { get; init; }    
    public required string Name { get; init; }

    public static RoleResponse Create(Role role)
    {
        var roleResponse = new RoleResponse
        {
            Id = role.Id.Value,
            Name = role.Name
        };
        
        return roleResponse;
    }
}