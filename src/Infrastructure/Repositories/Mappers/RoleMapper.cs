using Domain.Roles;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

public class RoleMapper : IMapper<Role, RoleDto>
{
    public RoleDto ToDto(Role role) => new (role.Id, role.Name);

    public Role ToEntity(RoleDto dto)
    {
        var role = new Role(dto.Name);
        role.SetId(dto.Id);
        
        return role;
    }
}