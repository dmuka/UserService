using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.RefreshTokens;
using Domain.Users;
using Microsoft.Extensions.Logging;

namespace Application.Users.SignIn;

internal sealed class SignInUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    ILogger<SignInUserCommandHandler> logger) : ICommandHandler<SignInUserCommand, SignInResponse>
{
    public async Task<Result<SignInResponse>> Handle(SignInUserCommand command, CancellationToken cancellationToken)
    {
        var user = command.Email is null 
            ? await userRepository.GetUserByUsernameAsync(command.Username, cancellationToken)
            : await userRepository.GetUserByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            var errorMessage = command.Email is null
                ? UserErrors.NotFoundByUsername(command.Username)
                : UserErrors.NotFoundByEmail(command.Email);
            logger.LogWarning("User not found: {ErrorMessage}", errorMessage);
            
            return Result.Failure<SignInResponse>(errorMessage);
        }

        var isPasswordCorrect = passwordHasher.CheckPassword(command.Password, user.PasswordHash);

        if (!isPasswordCorrect)
        {
            logger.LogWarning("Incorrect password for user: {Username}", user.Username);
            
            return Result.Failure<SignInResponse>(UserErrors.WrongPassword());
        }

        var accessToken = await tokenProvider.CreateAccessTokenAsync(user, command.RememberMe, cancellationToken);

        var validRefreshToken = await refreshTokenRepository.GetTokenByUserAsync(user, cancellationToken);

        if (validRefreshToken is null || validRefreshToken.ExpiresUtc < DateTime.UtcNow)
        {
            var result = RefreshToken.Create(
                Guid.CreateVersion7(),
                tokenProvider.CreateRefreshToken(),
                tokenProvider.GetExpirationValue(command.TokenExpirationInDays, ExpirationUnits.Minute, command.RememberMe),
                user.Id);

            if (result.IsFailure)
            {
                logger.LogError("Failed to create refresh token for user: {Username}", user.Username);
                
                return Result.Failure<SignInResponse>(result.Error);
            }

            validRefreshToken = result.Value;

            await refreshTokenRepository.AddTokenAsync(validRefreshToken, cancellationToken);
        }

        return new SignInResponse(accessToken, validRefreshToken.Id.Value);
    }
}
