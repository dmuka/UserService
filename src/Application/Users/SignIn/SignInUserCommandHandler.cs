using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.RefreshTokens;
using Domain.Users;

namespace Application.Users.SignIn;

internal sealed class SignInUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<SignInUserCommand, SignInResponse>
{
    public async Task<Result<SignInResponse>> Handle(SignInUserCommand command, CancellationToken cancellationToken)
    {
        var user = command.Email is null 
            ? await userRepository.GetUserByUsernameAsync(command.Username, cancellationToken)
            : await userRepository.GetUserByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            return command.Email is null
                ? Result.Failure<SignInResponse>(UserErrors.NotFoundByUsername(command.Username))
                : Result.Failure<SignInResponse>(UserErrors.NotFoundByEmail(command.Email));
        }

        var isPasswordCorrect = passwordHasher.CheckPassword(command.Password, user.PasswordHash);

        if (!isPasswordCorrect) return Result.Failure<SignInResponse>(UserErrors.WrongPassword());

        var accessToken = await tokenProvider.CreateAccessTokenAsync(user, cancellationToken);

        var result = RefreshToken.Create(
            Guid.CreateVersion7(),
            tokenProvider.CreateRefreshToken(),
            DateTime.UtcNow.AddHours(1),
            user.Id);

        if (result.IsFailure) return Result.Failure<SignInResponse>(result.Error);
        
        var refreshToken = result.Value;
        
        await refreshTokenRepository.AddTokenAsync(refreshToken, cancellationToken);

        return new SignInResponse(accessToken, refreshToken.Id.Value);
    }
}
