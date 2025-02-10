using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.SignInByToken;

internal sealed class SignInUserByTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenProvider tokenProvider) : ICommandHandler<SignInUserByTokenCommand, SignInUserByTokenResponse>
{
    public async Task<Result<SignInUserByTokenResponse>> Handle(SignInUserByTokenCommand command, CancellationToken cancellationToken)
    {
        var refreshToken = await refreshTokenRepository.GetTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken == null || refreshToken.ExpiresUtc <= DateTime.UtcNow)
        {
            throw new ApplicationException("Refresh token has expired.");
        }
        
        var accessToken = tokenProvider.CreateAccessToken(refreshToken.User);

        refreshToken.Value = tokenProvider.CreateRefreshToken();
        refreshToken.ExpiresUtc = DateTime.UtcNow.AddDays(7);
        
        await refreshTokenRepository.UpdateTokenAsync(refreshToken, cancellationToken);

        return new SignInUserByTokenResponse(accessToken, refreshToken.Value);
    }
}
