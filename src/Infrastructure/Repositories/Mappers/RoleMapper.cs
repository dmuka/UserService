using Domain.Roles;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

public class RoleMapper : IMapper<Role, RoleDto>
{
    public RoleDto ToDto(Role role) => new RoleDto { Id = role.Id.Value, Name = role.Name };

    public Role ToEntity(RoleDto dto)
    {
        var role = new Role(new RoleId(dto.Id), dto.Name);
        
        return role;
    }
}