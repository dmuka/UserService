using Core;
using Domain.Roles.Constants;

namespace Domain.Roles;

public static class RoleErrors
{
    public static Error NotFound(Guid roleId) => Error.NotFound(
        Codes.NotFound, 
        $"The role with the id = '{roleId}' was not found.");
    
    public static Error NotFound(string roleName) => Error.NotFound(
        Codes.NotFound, 
        $"The role with the role name = '{roleName}' was not found.");

    public static Error Unauthorized() => Error.Failure(
        Codes.Unauthorized,
        "You are not authorized to perform this action.");

    public static readonly Error RoleNameAlreadyExists = Error.Conflict(
        Codes.RoleNameExists,
        "The provided role name already exists.");

    public static readonly Error InvalidRoleName = Error.Problem(
        Codes.InvalidRoleName,
        "The provided role name is invalid.");

    public static readonly Error UsersWithAssignedRole = Error.Problem(
        Codes.UsersWithAssignedRole,
        "Can't remove role with users assigned to it.");
}