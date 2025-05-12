using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.EnableMfa;

internal sealed class EnableMfaCommandHandler(IUserRepository userRepository, ITotpProvider totpProvider) : ICommandHandler<EnableMfaCommand, List<string>>
{
    public async Task<Result<List<string>>> Handle(EnableMfaCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userId))
        {
            return Result.Failure<List<string>>(UserErrors.InvalidUserId);
        }
        
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure<List<string>>(UserErrors.NotFound(userId));

        var isValidCode = totpProvider.ValidateTotp(user.MfaSecret?.Value ?? "", command.VerificationCode);

        if (!isValidCode) return Result.Failure<List<string>>(UserErrors.InvalidVerificationCode);
        
        var result = user.EnableMfa();
        if (result.IsFailure) return Result.Failure<List<string>>(UserErrors.InvalidMfaState);
        
        await userRepository.UpdateUserAsync(user, cancellationToken);

        var recoveryCodesHashes = user.MfaState.RecoveryCodes;
        
        return recoveryCodesHashes.ToList();
    }
}