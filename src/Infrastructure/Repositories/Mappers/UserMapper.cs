﻿using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.RoleNames;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories.Mappers;

public class UserMapper(IRoleRepository roleRepository) : IMapper<User, UserId, UserDto>
{
    public UserDto ToDto(User user) => 
        new()
        {
            Id = user.Id.Value,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsMfaEnabled = user.IsMfaEnabled,
            MfaSecret = user.MfaSecret ?? (string?)null,
            IsEmailConfirmed = user.IsEmailConfirmed,
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
            new List<RoleName> { RoleName.Create(role?.Name ?? defaultUserRole) },
            new List<UserPermissionId>(),
            dto.RecoveryCodesHashes,
            dto.IsMfaEnabled,
            dto.MfaSecret).Value;
        
        return user;
    }
}