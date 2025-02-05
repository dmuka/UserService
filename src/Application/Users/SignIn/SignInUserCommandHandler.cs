using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.SignIn;

internal sealed class SignInUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<SignInUserCommand, string>
{
    public async Task<Result<string>> Handle(SignInUserCommand command, CancellationToken cancellationToken)
    {
        var user = command.Email is null 
            ? await userRepository.GetUserByUsernameAsync(command.Username, cancellationToken)
            : await userRepository.GetUserByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            return command.Email is null
                ? Result.Failure<string>(UserErrors.NotFoundByUsername(command.Username))
                : Result.Failure<string>(UserErrors.NotFoundByEmail(command.Email));
        }

        var isPasswordCorrect = passwordHasher.CheckPassword(command.Password, user.PasswordHash);

        if (!isPasswordCorrect) return Result.Failure<string>(UserErrors.WrongPassword());

        var token = tokenProvider.Create(user);

        return token;
    }
}
