using Core;
using Domain.ValueObjects.MfaSecrets;

namespace Domain.ValueObjects.MfaState;

/// <summary>
/// Represents the MFA state of a user, encapsulating the MFA secret, recovery codes hashes and enabled status.
/// </summary>
public sealed class MfaState : ValueObject
{
    public bool IsEnabled { get; private set; }
    public MfaSecret? Secret { get; private set; }
    public DateTime? LastVerificationDate { get; private set; }
    public IReadOnlyCollection<string> RecoveryCodesHashes => _recoveryCodesHashes.AsReadOnly();
    
    private List<string> _recoveryCodesHashes;

    private MfaState(
        bool isEnabled, 
        MfaSecret? secret,
        DateTime? lastVerificationDate,
        List<string> recoveryCodesHashes)
    {
        IsEnabled = isEnabled;
        Secret = secret;
        LastVerificationDate = lastVerificationDate;
        _recoveryCodesHashes = recoveryCodesHashes;
    }

    /// <summary>
    /// Creates a disabled MFA state.
    /// </summary>
    public static MfaState Disabled() => new(false, null, null, []);

    /// <summary>
    /// Creates an MFA state with the secret set but not yet enabled and empty collection of the recovery codes hashes.
    /// </summary>
    /// <param name="secret">The MFA secret.</param>
    /// <param name="recoveryCodesHashes">Collection of the recovery codes hashes.</param>
    public static Result<MfaState> WithArtifacts(MfaSecret? secret, ICollection<string>? recoveryCodesHashes)
    {
        return secret is null || recoveryCodesHashes is null || recoveryCodesHashes.Count == 0
            ? Result.Failure<MfaState>(Error.NullValue) 
            : new MfaState(false, secret, null, recoveryCodesHashes.ToList());
    }

    /// <summary>
    /// Creates an enabled MFA state with the provided secret and collection of the recovery codes hashes.
    /// </summary>
    /// <param name="secret">The MFA secret.</param>
    /// <param name="recoveryCodesHashes">Collection of the recovery codes hashes.</param>
    public static Result<MfaState> Enabled(MfaSecret? secret, ICollection<string> recoveryCodesHashes)
    {
        return secret is null || recoveryCodesHashes.Count == 0
            ? Result.Failure<MfaState>(Users.UserErrors.InvalidMfaState) : 
            new MfaState(true, secret, DateTime.UtcNow, recoveryCodesHashes.ToList());
    }

    /// <summary>
    /// Enables the MFA if a secret is present.
    /// </summary>
    public Result<MfaState> Enable()
    {
        return Secret is null || _recoveryCodesHashes.Count == 0
            ? Result.Failure<MfaState>(Users.UserErrors.InvalidMfaState) 
            : new MfaState(true, Secret, DateTime.UtcNow, _recoveryCodesHashes);
    }

    /// <summary>
    /// Disables the MFA.
    /// </summary>
    public static MfaState Disable() => Disabled();

    /// <summary>
    /// Adds the recovery code hash while keeping the enabled status the same.
    /// </summary>
    /// <param name="hash">The new recovery code.</param>
    public MfaState AddRecoveryCodeHash(string hash)
    {
        var hashes = new List<string>(_recoveryCodesHashes) { hash };
        
        return new MfaState(IsEnabled, Secret, LastVerificationDate, hashes);
    }

    /// <summary>
    /// Removes the recovery code hash while keeping the enabled status the same.
    /// </summary>
    /// <param name="hash">The new recovery code.</param>
    public MfaState RemoveRecoveryCodeHash(string hash)
    {
        var hashes = new List<string>(_recoveryCodesHashes.Except([hash]));
        
        return new MfaState(IsEnabled, Secret, LastVerificationDate, hashes);
    }

    /// <summary>
    /// Checks if the MFA state is valid (either disabled or has a secret and at least one recovery code hash if enabled).
    /// </summary>
    public bool IsValid()
    {
        return (IsEnabled && Secret is not null && _recoveryCodesHashes.Count != 0) || !IsEnabled;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsEnabled;
        if (Secret != null) yield return Secret;
        if (LastVerificationDate.HasValue) yield return LastVerificationDate.Value;
        foreach (var hash in _recoveryCodesHashes)
        {
            yield return hash;
        }
    }
}