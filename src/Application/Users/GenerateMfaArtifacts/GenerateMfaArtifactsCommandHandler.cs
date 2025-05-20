using System.Reflection;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;
using Domain.ValueObjects.MfaSecrets;

namespace Application.Users.GenerateMfaArtifacts;

internal sealed class GenerateMfaArtifactsCommandHandler(
    IUserRepository userRepository, 
    ITotpProvider totpProvider,
    IRecoveryCodesProvider recoveryCodesProvider) : ICommandHandler<GenerateMfaArtifactsCommand, (string qr, List<string> codes)>
{
    public async Task<Result<(string qr, List<string> codes)>> Handle(GenerateMfaArtifactsCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userId))
        {
            return Result.Failure<(string, List<string>)>(UserErrors.InvalidUserId);
        }
        
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure<(string, List<string>)>(UserErrors.NotFound(userId));

        if (!user.IsEmailConfirmed) return Result.Failure<(string, List<string>)>(UserErrors.UserEmailNotConfirmedYet);
        
        List<string> codes = [];
        
        if (!user.IsMfaEnabled)
        {
            var secretKey = totpProvider.GenerateSecretKey();
            var recoveries = recoveryCodesProvider.GenerateRecoveryCodes();
            codes = recoveries.Select(recovery => recovery.code).ToList();
            var hashes = recoveries.Select(recovery => recovery.hashCode).ToList();
                
            var result = user.SetupMfa(MfaSecret.Create(secretKey), hashes);
            if (result.IsFailure) return Result.Failure<(string, List<string>)>(UserErrors.InvalidMfaSecret);
            await userRepository.UpdateUserAsync(user, cancellationToken);
        }
        
        var qr = totpProvider.GetQr(user.MfaSecret ?? "", user.Email, Assembly.GetEntryAssembly()?.GetName().Name ?? "Standards");
            
        return (qr, codes);
    }
}