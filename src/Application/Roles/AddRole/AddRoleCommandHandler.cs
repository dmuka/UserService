using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;

namespace Application.Roles.AddRole;

public class AddRoleCommandHandler(IRoleRepository repository, IUserContext userContext) 
    : ICommandHandler<AddRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        AddRoleCommand command, 
        CancellationToken cancellationToken)
    {
        if (await repository.IsRoleNameExistsAsync(command.Name, cancellationToken))
        {
            return Result.Failure<Guid>(RoleErrors.RoleNameAlreadyExists);
        }
        
        var role = Role.Create(Guid.CreateVersion7(), command.Name);
        
        await repository.AddRoleAsync(role, cancellationToken);

        return role.Id.Value;
    }
}