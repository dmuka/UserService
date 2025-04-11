using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories.Mappers;

public class UserMapper(IRoleRepository roleRepository) : IMapper<User, UserDto>
{
    public UserDto ToDto(User user) => 
        new()
        {
            Id = user.Id.Value,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PasswordHash = user.PasswordHash,
            Email = user.Email
        };

    public User ToEntity(UserDto dto)
    {
        var defaultUserRole = new ConfigurationManager().GetSection("DefaultUserRole").Value ?? "User";
        var role = roleRepository.GetRoleByNameAsync(defaultUserRole).Result;
        
        var user = User.Create(
            dto.Id, 
            dto.Username, 
            dto.FirstName, 
            dto.LastName, 
            dto.PasswordHash, 
            dto.Email, 
            new List<RoleId> { role.Id },
            new List<UserPermissionId>()).Value;
        
        return user;
    }
}