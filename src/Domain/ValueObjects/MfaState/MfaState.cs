using Core;
using Domain.ValueObjects.MfaSecrets;

namespace Domain.ValueObjects.MfaState;

/// <summary>
/// Represents the MFA state of a user, encapsulating the MFA secret and enabled status.
/// </summary>
public sealed class MfaState : ValueObject
{
    public bool IsEnabled { get; private set; }
    public MfaSecret? Secret { get; private set; }
    public DateTime? LastVerificationDate { get; private set; }
    public IReadOnlyCollection<string> RecoveryCodes => _recoveryCodes.AsReadOnly();
    
    private List<string> _recoveryCodes;

    private MfaState(
        bool isEnabled, 
        MfaSecret? secret,
        DateTime? lastVerificationDate,
        List<string> recoveryCodes)
    {
        IsEnabled = isEnabled;
        Secret = secret;
        LastVerificationDate = lastVerificationDate;
        _recoveryCodes = recoveryCodes;
    }

    /// <summary>
    /// Creates a disabled MFA state.
    /// </summary>
    public static MfaState Disabled() => new(false, null, null, []);

    /// <summary>
    /// Creates an MFA state with the secret set but not yet enabled and empty collection of the recovery codes.
    /// </summary>
    /// <param name="secret">The MFA secret.</param>
    /// <param name="recoveryCodesHashes">Collection of the recovery codes.</param>
    public static Result<MfaState> WithArtifacts(MfaSecret? secret, ICollection<string>? recoveryCodesHashes)
    {
        return secret is null || recoveryCodesHashes is null || recoveryCodesHashes.Count == 0
            ? Result.Failure<MfaState>(Error.NullValue) 
            : new MfaState(false, secret, null, recoveryCodesHashes.ToList());
    }

    /// <summary>
    /// Creates an enabled MFA state with the provided secret and collection of the recovery codes.
    /// </summary>
    /// <param name="secret">The MFA secret.</param>
    /// <param name="recoveryCodes">Collection of the recovery codes.</param>
    public static Result<MfaState> Enabled(MfaSecret? secret, ICollection<string> recoveryCodes)
    {
        return secret is null || recoveryCodes.Count == 0
            ? Result.Failure<MfaState>(Users.UserErrors.InvalidMfaState) : 
            new MfaState(true, secret, DateTime.UtcNow, recoveryCodes.ToList());
    }

    /// <summary>
    /// Enables the MFA if a secret is present.
    /// </summary>
    public Result<MfaState> Enable()
    {
        return Secret is null || _recoveryCodes.Count == 0
            ? Result.Failure<MfaState>(Users.UserErrors.InvalidMfaState) 
            : new MfaState(true, Secret, DateTime.UtcNow, _recoveryCodes);
    }

    /// <summary>
    /// Disables the MFA.
    /// </summary>
    public static MfaState Disable() => Disabled();

    /// <summary>
    /// Adds the recovery code while keeping the enabled status the same.
    /// </summary>
    /// <param name="code">The new recovery code.</param>
    public MfaState AddRecoveryCode(string code)
    {
        var codes = new List<string>(_recoveryCodes) { code };
        
        return new MfaState(IsEnabled, Secret, LastVerificationDate, codes);
    }

    /// <summary>
    /// Checks if the MFA state is valid (either disabled or has a secret and at least one recovery code if enabled).
    /// </summary>
    public bool IsValid()
    {
        return (IsEnabled && Secret != null && _recoveryCodes.Count != 0) || !IsEnabled;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsEnabled;
        if (Secret != null) yield return Secret;
        if (LastVerificationDate.HasValue) yield return LastVerificationDate.Value;
        foreach (var code in _recoveryCodes)
        {
            yield return code;
        }
    }
}