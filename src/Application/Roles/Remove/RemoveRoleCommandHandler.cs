using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;

namespace Application.Roles.Remove;

public class RemoveRoleCommandHandler(
    IRoleRepository repository,
    IUserRoleRepository userRoleRepository,
    IUserContext userContext) 
    : ICommandHandler<RemoveRoleCommand, int>
{
    public async Task<Result<int>> Handle(
        RemoveRoleCommand command, 
        CancellationToken cancellationToken)
    {
        var usersWithRole = await userRoleRepository.GetUsersIdsByRoleIdAsync(command.RoleId, cancellationToken);
        if (usersWithRole.Count > 0) return Result.Failure<int>(RoleErrors.UsersWithAssignedRole);
        
        var rowsCount = await repository.RemoveRoleByIdAsync(command.RoleId, cancellationToken);

        return rowsCount > 0 ? Result.Success(rowsCount) : Result.Failure<int>(RoleErrors.NotFound(command.RoleId));
    }
}