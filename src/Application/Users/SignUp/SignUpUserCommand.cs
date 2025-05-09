using Application.Abstractions.Messaging;
using Domain.UserPermissions;

namespace Application.Users.SignUp;

public sealed record SignUpUserCommand(
    string Username,
    string Email, 
    string FirstName, 
    string LastName, 
    string Password,
    bool IsMfaEnabled,
    string? MfaSecret,
    IEnumerable<Guid>? RolesIds = null,
    IEnumerable<UserPermissionId>? UserPermissionIds = null,
    IEnumerable<string>? RecoveryCodes = null)
    : ICommand<Guid>;
