using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Users.Update;

public class UpdateUserCommandHandler(
    IUserRepository repository, 
    IUserRoleRepository userRoleRepository,
    IUserContext userContext) 
    : ICommandHandler<UpdateUserCommand, int>
{
    public async Task<Result<int>> Handle(
        UpdateUserCommand command, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserRole != "Admin")
        {
            return Result.Failure<int>(UserErrors.Unauthorized());
        }
        
        await repository.UpdateUserAsync(command.User, cancellationToken);
        
        var roleIds = command.User.RoleIds.Select(roleId => roleId.Value);
        var result =await userRoleRepository.UpdateUserRolesAsync(command.User.Id.Value, roleIds, cancellationToken);

        return result;
    }
}