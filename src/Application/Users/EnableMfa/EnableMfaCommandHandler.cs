using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.EnableMfa;

internal sealed class EnableMfaCommandHandler(IUserRepository userRepository, ITotpProvider totpProvider) : ICommandHandler<EnableMfaCommand>
{
    public async Task<Result> Handle(EnableMfaCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userId))
        {
            return Result.Failure(UserErrors.InvalidUserId);
        }
        
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));

        var isValidCode = totpProvider.ValidateTotp(user.MfaSecret.Value, command.VerificationCode);

        if (!isValidCode) return Result.Failure(UserErrors.InvalidVerificationCode);
        
        var result = user.EnableMfa();
        if (result.IsFailure) return Result.Failure(UserErrors.InvalidVerificationCode);
        
        await userRepository.UpdateUserAsync(user, cancellationToken);
        
        return Result.Success();
    }
}