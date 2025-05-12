using System.Reflection;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;
using Domain.ValueObjects.MfaSecrets;

namespace Application.Users.GenerateQr;

internal sealed class GenerateQrCommandHandler(
    IUserRepository userRepository, 
    ITotpProvider totpProvider,
    IRecoveryCodesProvider recoveryCodesProvider) : ICommandHandler<GenerateQrCommand, string>
{
    public async Task<Result<string>> Handle(GenerateQrCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userId))
        {
            return Result.Failure<string>(UserErrors.InvalidUserId);
        }
        
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure<string>(UserErrors.NotFound(userId));

        if (user.MfaSecret is null)
        {
            var secretKey = totpProvider.GenerateSecretKey();
            var recoveries = recoveryCodesProvider.GenerateRecoveryCodes();
            var codes = recoveries.Select(recovery => recovery.code).ToList();
            var hashes = recoveries.Select(recovery => recovery.hashCode).ToList();
                
            var result = user.SetupMfa(MfaSecret.Create(secretKey), hashes);
            if (result.IsFailure) return Result.Failure<string>(UserErrors.InvalidMfaSecret);
            await userRepository.UpdateUserAsync(user, cancellationToken);
        }
        
        var qr = totpProvider.GetQr(user.MfaSecret ?? "", user.Email, Assembly.GetEntryAssembly()?.GetName().Name ?? "Standards");
            
        return qr;
    }
}