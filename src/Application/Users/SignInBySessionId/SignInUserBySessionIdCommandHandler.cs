using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.RefreshTokens;
using Domain.Users;

namespace Application.Users.SignInBySessionId;

public sealed class SignInUserBySessionIdCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenProvider tokenProvider) : ICommandHandler<SignInUserBySessionIdCommand, SignInUserBySessionIdResponse>
{
    public async Task<Result<SignInUserBySessionIdResponse>> Handle(SignInUserBySessionIdCommand command, CancellationToken cancellationToken)
    {
        var result = await refreshTokenRepository.GetTokenByIdAsync(command.SessionId, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<SignInUserBySessionIdResponse>(RefreshTokenErrors.NotFound(command.SessionId));
        }
        
        var refreshToken = result.Value;
        
        var user = await userRepository.GetUserByIdAsync(refreshToken.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure<SignInUserBySessionIdResponse>(UserErrors.NotFound(refreshToken.UserId.Value));
        }
        
        var accessToken = await tokenProvider.CreateAccessTokenAsync(user, false, cancellationToken);

        return new SignInUserBySessionIdResponse(accessToken);
    }
}
