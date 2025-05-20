using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Users.Remove;

public class RemoveUserCommandHandler(
    IUserRepository repository,
    IUserRoleRepository userRoleRepository) 
    : ICommandHandler<RemoveUserCommand, int>
{
    public async Task<Result<int>> Handle(
        RemoveUserCommand command, 
        CancellationToken cancellationToken)
    {
        await userRoleRepository.RemoveAllUserRolesAsync(command.UserId, cancellationToken);
        var rowsCount = await repository.RemoveUserByIdAsync(command.UserId, cancellationToken);

        return rowsCount > 0 
            ? Result.Success(rowsCount) 
            : Result.Failure<int>(UserErrors.NotFound(command.UserId));
    }
}