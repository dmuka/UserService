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
        var refreshToken = await refreshTokenRepository.GetTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            return Result.Failure<SignInUserByTokenResponse>(RefreshTokenErrors.NotFoundByValue(command.RefreshToken));
        }

        if (refreshToken.ExpiresUtc <= DateTime.UtcNow)
        {
            await refreshTokenRepository.RemoveExpiredTokensAsync(cancellationToken);
            
            RefreshToken.Create(
                Guid.CreateVersion7(), 
                tokenProvider.CreateRefreshToken(),
                DateTime.UtcNow.AddDays(command.TokenExpirationInDays),
                refreshToken.UserId);
            
            return Result.Failure<SignInUserByTokenResponse>(RefreshTokenErrors.InvalidExpiresDate);
        }
        
        var user = await userRepository.GetUserByIdAsync(refreshToken.UserId.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<SignInUserByTokenResponse>(UserErrors.NotFound(refreshToken.UserId.Value));
        }
        
        var accessToken = await tokenProvider.CreateAccessTokenAsync(user, cancellationToken);

        refreshToken.ChangeValue(tokenProvider.CreateRefreshToken());
        refreshToken.ChangeExpireDate(DateTime.UtcNow.AddDays(7));
        
        await refreshTokenRepository.UpdateTokenAsync(refreshToken, cancellationToken);

        return new SignInUserByTokenResponse(accessToken, refreshToken.Id.Value);
    }
}
