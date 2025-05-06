using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;
using Domain.ValueObjects.PasswordHashes;

namespace Application.Users.ResetPassword;

public class ResetPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);
        if (user is null) return Result.Failure<User>(UserErrors.NotFound(command.UserId));
        
        var passwordHash = passwordHasher.GetHash(command.Password);

        var result = user.ChangePassword(PasswordHash.Create(passwordHash));
        if (result.IsFailure) return Result.Failure<User>(UserErrors.WrongPassword());
        
        await userRepository.UpdateUserAsync(user, cancellationToken);
            
        return Result.Success();
    }
}