using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using RoleConstants = Domain.Roles.Constants.Roles;

namespace Application.Users.SignUp;

internal sealed class SignUpUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<SignUpUserCommand, Guid>
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

        var passwordHash = passwordHasher.GetHash(command.Password);
        
        var user = User.CreateUser(
            Guid.CreateVersion7(),
            command.Username, 
            command.FirstName, 
            command.LastName, 
            new PasswordHash(passwordHash),
            new Email(command.Email),
            new List<Role> { defaultUserRole! });

        var userId = await userRepository.AddUserAsync(user, cancellationToken);

        return userId;
    }
}
