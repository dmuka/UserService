using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Users.Update;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Roles.Update;

public class UpdateRoleCommandHandler(IRoleRepository repository, IUserContext userContext) 
    : ICommandHandler<UpdateRoleCommand, int>
{
    public async Task<Result<int>> Handle(
        UpdateRoleCommand command, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserRole != "Admin")
        {
            return Result.Failure<int>(RoleErrors.Unauthorized());
        }
        
        var result = await repository.UpdateRoleAsync(command.Role, cancellationToken);

        return result;
    }
}