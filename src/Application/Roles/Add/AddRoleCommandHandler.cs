using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain;
using Domain.Roles;

namespace Application.Roles.Add;

public class AddRoleCommandHandler(
    IRoleRepository repository, 
    IUserContext userContext,
    IEventDispatcher eventDispatcher) 
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

        if (role.IsFailure) return Result.Failure<Guid>(role.Error);
        
        var roleId = await repository.AddRoleAsync(role.Value, cancellationToken);

        foreach (var domainEvent in role.Value.DomainEvents)
        {
            await eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
        role.Value.ClearDomainEvents();

        return roleId;
    }
}