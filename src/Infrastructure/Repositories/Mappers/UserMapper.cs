using Core;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

public class UserMapper : IMapper<User, UserDto>
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
        var user = User.CreateUser(
            dto.Id, 
            dto.Username, 
            dto.FirstName, 
            dto.LastName, 
            new PasswordHash(dto.PasswordHash), 
            new Email(dto.Email), 
            new List<Role>());
        
        return user;
    }
}