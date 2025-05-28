using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain;
using Domain.Users;
using Domain.Users.Events.Domain;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.RoleNames;
using RoleConstants = Domain.Roles.Constants.Roles;

namespace Application.Users.SignUp;

internal sealed class SignUpUserCommandHandler(
    IUserRepository userRepository,
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

        IList<RoleName>? roleNames = null;
        
        if (command.RolesNames is not null) 
        {
            roleNames = command.RolesNames.Select(roleName => RoleName.Create(roleName).Value).ToList();
        }
        
        var passwordHash = passwordHasher.GetHash(command.Password);
        
        var user = User.Create(
            Guid.CreateVersion7(),
            command.Username, 
            command.FirstName, 
            command.LastName, 
            passwordHash,
            command.Email,
            roleNames ?? [RoleName.Create(RoleConstants.DefaultUserRole).Value],
            command.UserPermissionIds?.ToList()
            );

        if (user.IsFailure) return Result.Failure<Guid>(user.Error);

        var userId = await userRepository.AddUserAsync(user.Value, cancellationToken);

        var @event =
            new UserRegisteredDomainEvent(new UserId(userId), Email.Create(command.Email), DateTime.UtcNow);
        
        await eventDispatcher.DispatchAsync(@event, cancellationToken);

        return userId;
    }
}
