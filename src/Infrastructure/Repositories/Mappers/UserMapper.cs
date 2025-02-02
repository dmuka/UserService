using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

public class UserMapper : IMapper<User, UserDto>
{
    public UserDto ToDto(User user) => 
        new (user.Id.Value,
            user.Username,
            user.FirstName, 
            user.LastName,
            user.PasswordHash,
            user.Email,
            user.Role.Id.Value,
            user.Role.Name);

    public User ToEntity(UserDto dto)
    {
        var user = new User(
            new UserId(dto.Id), 
            dto.Username, 
            dto.FirstName, 
            dto.LastName, 
            new PasswordHash(dto.PasswordHash), 
            new Email(dto.Email), 
            new Role(new RoleId(dto.RoleId), dto.RoleName));
        
        return user;
    }
}