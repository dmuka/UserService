using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.RefreshTokens;
using Domain.Users;

namespace Application.Users.SignInByToken;

public sealed class SignInUserByTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenProvider tokenProvider) : ICommandHandler<SignInUserByTokenCommand, SignInUserByTokenResponse>
{
    public async Task<Result<SignInUserByTokenResponse>> Handle(SignInUserByTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await refreshTokenRepository.GetTokenAsync(command.RefreshToken, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<SignInUserByTokenResponse>(RefreshTokenErrors.NotFoundByValue(command.RefreshToken));
        }
        
        var refreshToken = result.Value;

        if (refreshToken.ExpiresUtc <= DateTime.UtcNow)
        {
            await refreshTokenRepository.RemoveExpiredTokensAsync(cancellationToken);
            
            return Result.Failure<SignInUserByTokenResponse>(RefreshTokenErrors.InvalidExpiresDate);
        }
        
        var user = await userRepository.GetUserByIdAsync(refreshToken.UserId.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<SignInUserByTokenResponse>(UserErrors.NotFound(refreshToken.UserId.Value));
        }
        
        var accessToken = await tokenProvider.CreateAccessTokenAsync(user, false, cancellationToken);

        refreshToken.ChangeValue(tokenProvider.CreateRefreshToken());
        var exp = tokenProvider.GetExpirationValue(command.TokenExpirationInDays, ExpirationUnits.Day);
        refreshToken.ChangeExpireDate(tokenProvider.GetExpirationValue(command.TokenExpirationInDays, ExpirationUnits.Day));
        
        await refreshTokenRepository.UpdateTokenAsync(refreshToken, cancellationToken);

        return new SignInUserByTokenResponse(accessToken, refreshToken.Id.Value);
    }
}
