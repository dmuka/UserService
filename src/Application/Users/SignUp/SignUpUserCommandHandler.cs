using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using RoleConstants = Domain.Roles.Constants.Roles;

namespace Application.Users.SignUp;

internal sealed class SignUpUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IEventDispatcher eventDispatcher) : ICommandHandler<SignUpUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SignUpUserCommand command, CancellationToken cancellationToken)
    {
        if (await userRepository.IsUsernameExistsAsync(command.Username, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.UsernameAlreadyExists);
        }
        
        if (await userRepository.IsEmailExistsAsync(command.Email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);
        }

        var defaultUserRole = await roleRepository.GetRoleByNameAsync(RoleConstants.DefaultUserRole, cancellationToken);

        var roleIds = new List<RoleId>();
        
        if (command.RolesIds is not null) roleIds = command.RolesIds.Select(roleId => new RoleId(roleId)).ToList();
        
        var passwordHash = passwordHasher.GetHash(command.Password);
        
        var user = User.CreateUser(
            Guid.CreateVersion7(),
            command.Username, 
            command.FirstName, 
            command.LastName, 
            passwordHash,
            command.Email,
            command.RolesIds is null ? [defaultUserRole.Id] : roleIds,
            new List<UserPermissionId>());

        if (user.IsFailure) return Result.Failure<Guid>(user.Error);

        var userId = await userRepository.AddUserAsync(user.Value, cancellationToken);

        foreach (var domainEvent in user.Value.DomainEvents)
        {
            await eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
        user.Value.ClearDomainEvents();

        return userId;
    }
}
