using Domain.Roles;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

public class RoleMapper : IMapper<Role, RoleDto>
{
    public RoleDto ToDto(Role role) => new() { Id = role.Id.Value, Name = role.Name };

    public Role ToEntity(RoleDto dto)
    {
        var role = Role.CreateRole(dto.Id, dto.Name);
        
        return role;
    }
}