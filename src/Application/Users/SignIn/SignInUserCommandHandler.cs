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
    ITotpProvider totpProvider,
    IRecoveryCodesProvider recoveryCodesProvider,
    ILogger<SignInUserCommandHandler> logger) : ICommandHandler<SignInUserCommand, SignInResponse>
{
    public async Task<Result<SignInResponse>> Handle(SignInUserCommand command, CancellationToken cancellationToken)
    {
        var user = string.IsNullOrEmpty(command.Email) 
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

        if (!string.IsNullOrEmpty(command.Password))
        {
            var isPasswordCorrect = passwordHasher.CheckPassword(command.Password, user.PasswordHash);

            if (!isPasswordCorrect)
            {
                logger.LogWarning("Incorrect password for user: {Username}", user.Username);

                return Result.Failure<SignInResponse>(UserErrors.WrongPassword());
            }
        } 
        else if (user.IsMfaEnabled)
        {
            if (string.IsNullOrEmpty(command.VerificationCode) && string.IsNullOrEmpty(command.RecoveryCode))
            {
                return Result.Failure<SignInResponse>(UserErrors.MfaModeEnabled());
            }
            
            if (!string.IsNullOrEmpty(command.VerificationCode))
            {
                if (!int.TryParse(command.VerificationCode, out var code) ||
                    !totpProvider.ValidateTotp(user.MfaSecret ?? "", code))
                {
                    return Result.Failure<SignInResponse>(UserErrors.WrongVerificationCode());
                }
            }
            else if (!string.IsNullOrEmpty(command.RecoveryCode) 
                     && user.RecoveryCodesHashes is not null 
                     && user.RecoveryCodesHashes.Count > 0)
            {
                var usedHash = user.RecoveryCodesHashes.FirstOrDefault(hash =>
                    recoveryCodesProvider.VerifyRecoveryCode(hash, command.RecoveryCode));

                if (usedHash is null) return Result.Failure<SignInResponse>(UserErrors.WrongRecoveryCode());
                
                user.MfaState.RemoveRecoveryCodeHash(usedHash);

                await userRepository.UpdateUserAsync(user, cancellationToken);
            }
        }
        else
        {
            return Result.Failure<SignInResponse>(UserErrors.NoAuthData());
        }

        var accessToken = await tokenProvider.CreateAccessTokenAsync(user, command.RememberMe, cancellationToken);

        var validRefreshToken = await refreshTokenRepository.GetTokenByUserAsync(user, cancellationToken);

        if (validRefreshToken.IsFailure)
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

            await refreshTokenRepository.AddTokenAsync(validRefreshToken.Value, cancellationToken);
        }

        return new SignInResponse(accessToken, validRefreshToken.Value.Id.Value);
    }
}
