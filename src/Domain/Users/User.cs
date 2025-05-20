using Core;
using Domain.UserPermissions;
using Domain.Users.DomainEvents;
using Domain.Users.Specifications;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.MfaSecrets;
using Domain.ValueObjects.MfaState;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;

namespace Domain.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity<UserId>, IAggregationRoot
{
    public string Username { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public Email Email { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public MfaState MfaState { get; private set; }
    private List<RoleName> _roleNames = [];
    public IReadOnlyCollection<RoleName> RoleNames => _roleNames.AsReadOnly();
    private List<UserPermissionId> _userPermissionIds = [];
    public IReadOnlyCollection<UserPermissionId> UserPermissionIds => _userPermissionIds.AsReadOnly();
    
    public bool IsMfaEnabled => MfaState.IsEnabled;
    public MfaSecret? MfaSecret => MfaState.Secret;
    public IReadOnlyCollection<string>? RecoveryCodesHashes => MfaState.RecoveryCodesHashes;

    /// <summary>
    /// Default constructor for ORM compatibility.
    /// </summary>
    protected User() { }

    /// <summary>
    /// Creates a new instance of the <see cref="User"/> class using the specified details.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="userName">The username of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <param name="passwordHash">The hashed password of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="roleNames">A collection of the user's role names.</param>
    /// <param name="userPermissionIds">A collection of the user's permission IDs.</param>
    /// <param name="recoveryCodes">A collection of the user's recovery codes.</param>
    /// <param name="isMfaEnabled">Indicates whether multifactor authentication is enabled.</param>
    /// <param name="mfaSecret">The multifactor authentication secret value.</param>
    /// <returns>A <see cref="Result{TValue}"/> containing the created user or the validation errors.</returns>
    public static Result<User> Create(
        Guid userId,
        string userName,
        string firstName,
        string lastName,
        string passwordHash,
        string email,
        ICollection<RoleName> roleNames,
        ICollection<UserPermissionId>? userPermissionIds,
        ICollection<string>? recoveryCodes = null,
        bool isMfaEnabled = false,
        string? mfaSecret = null)
    {
        var resultsWithFailures = ValidateUserDetails(
            userName,
            firstName,
            lastName,
            passwordHash,
            email,
            roleNames,
            userPermissionIds,
            recoveryCodes,
            mfaSecret,
            isMfaEnabled);

        if (resultsWithFailures.Length != 0)
        {
            return Result<User>.ValidationFailure(ValidationError.FromResults(resultsWithFailures));
        }

        var mfaState = string.IsNullOrEmpty(mfaSecret) 
                       || recoveryCodes is null 
                       || recoveryCodes.Count == 0
            ? MfaState.Disabled()
            : isMfaEnabled
                ? MfaState.Enabled(MfaSecret.Create(mfaSecret), recoveryCodes).Value
                : MfaState.WithArtifacts(MfaSecret.Create(mfaSecret), recoveryCodes).Value;

        var user = new User(
            new UserId(userId), 
            userName, 
            firstName, 
            lastName, 
            PasswordHash.Create(passwordHash), 
            Email.Create(email),
            mfaState,
            roleNames,
            userPermissionIds);
    
        var userRegisteredEvent = new UserRegisteredDomainEvent(userId);
        user.AddDomainEvent(userRegisteredEvent);

        return user;
    }    
    
    private User(
        UserId userId,
        string userName,
        string firstName,
        string lastName,
        PasswordHash passwordHash, 
        Email email,
        MfaState mfaState,
        ICollection<RoleName> roleNames,
        ICollection<UserPermissionId>? userPermissionIds)
    {
        Id = userId;
        Username = userName;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Email = email;
        MfaState = mfaState;
        _roleNames = new List<RoleName>(roleNames);
        _userPermissionIds = userPermissionIds is not null 
            ? [..userPermissionIds] 
            : [];
    }

    /// <summary>
    /// Changes the email address of the user.
    /// </summary>
    /// <param name="newEmail">The new email address.</param>
    public Result ChangeEmail(Email? newEmail)
    {
        if (newEmail is null) return Result.Failure<Email>(Error.NullValue);
        
        Email = newEmail;
    
        var emailChangedEvent = new UserEmailChangedEvent(Id.Value, newEmail.Value);
        AddDomainEvent(emailChangedEvent);
    
        return Result.Success();
    }

    /// <summary>
    /// Changes the confirmation state of the email address of the user.
    /// </summary>
    public Result ConfirmEmail()
    {
        if (IsEmailConfirmed) return Result.Failure<Email>(UserErrors.EmailAlreadyConfirmed(Email));
        
        IsEmailConfirmed = true;
    
        return Result.Success();
    }

    /// <summary>
    /// Changes the password hash of the user.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    public Result ChangePassword(PasswordHash? newPasswordHash)
    {
        if (newPasswordHash is null) return Result.Failure<PasswordHash>(Error.NullValue);
        
        PasswordHash = newPasswordHash;
        
        return Result.Success();
    }

    /// <summary>
    /// Removes the role of the user.
    /// </summary>
    /// <param name="roleName">The role name to remove.</param>
    public Result RemoveRole(RoleName? roleName)
    {
        if (roleName is null) return Result.Failure<RoleName>(Error.NullValue);
        var validationResult = new UserMustHaveAtLeastOneRoleAfterRemoveRole(_roleNames).IsSatisfied();
        if (validationResult.IsFailure) return validationResult;
        
        _roleNames.Remove(roleName);
        
        return Result.Success();
    }

    /// <summary>
    /// Removes the permission of the user.
    /// </summary>
    /// <param name="userPermissionId">The permission to remove.</param>
    public Result RemovePermission(UserPermissionId? userPermissionId)
    {
        if (userPermissionId is null) return Result.Failure<UserPermissionId>(Error.NullValue);  
        
        _userPermissionIds.Remove(userPermissionId);
        
        return Result.Success();
    }

    /// <summary>
    /// Adds the role of the user.
    /// </summary>
    /// <param name="roleName">The role id to add.</param>
    public Result AddRole(RoleName? roleName)
    {
        if (roleName is null) return Result.Failure<RoleName>(Error.NullValue);
        
        _roleNames.Add(roleName); 
        
        return Result.Success();
    }

    /// <summary>
    /// Set-ups the MFA of the user.
    /// </summary>
    /// <param name="mfaSecret">The MFA secret key value.</param>
    /// <param name="recoveryCodesHashes">The collection of recovery codes hashes.</param>
    public Result SetupMfa(MfaSecret? mfaSecret, ICollection<string> recoveryCodesHashes)
    {
        if (mfaSecret is null) return Result.Failure<MfaSecret>(Error.NullValue);

        var result = MfaState.WithArtifacts(mfaSecret, recoveryCodesHashes);
        if (result.IsFailure) return Result.Failure(UserErrors.InvalidMfaState);
        
        MfaState = result.Value;
        
        return Result.Success();
    }

    /// <summary>
    /// Disables the MFA of the user.
    /// </summary>
    public Result DisableMfa()
    {
        var result = MfaState.Disable();
        
        return result.IsEnabled 
            ? Result.Failure(UserErrors.InvalidMfaState) 
            : Result.Success();
    }

    /// <summary>
    /// Enables the MFA of the user.
    /// </summary>
    public Result EnableMfa()
    {
        var result = MfaState.Enable();
        if (result.IsFailure) return Result.Failure(UserErrors.InvalidMfaState);
            
        MfaState = result.Value;
        
        return Result.Success();
    }

    /// <summary>
    /// Adds the permission to the user.
    /// </summary>
    /// <param name="userPermissionId">The permission id to add.</param>
    public Result AddPermission(UserPermissionId? userPermissionId)
    {
        if (userPermissionId is null) return Result.Failure<UserPermissionId>(Error.NullValue);
        
        _userPermissionIds.Add(userPermissionId); 
        
        return Result.Success();
    }

    /// <summary>
    /// Validates user details.
    /// </summary>
    private static Result[] ValidateUserDetails(
        string userName,
        string firstName,
        string lastName,
        string passwordHash,
        string email,
        ICollection<RoleName> roleNames,
        ICollection<UserPermissionId>? userPermissionIds,
        ICollection<string>? recoveryCodes,
        string? mfaSecret = null,
        bool isMfaEnabled = false)
    {
        var validationResults = new []
        {
            new UserNameMustBeValid(userName).IsSatisfied(),
            new FirstNameMustBeValid(firstName).IsSatisfied(),
            new LastNameMustBeValid(lastName).IsSatisfied(),
            new MustBeNonNullValue<string>(passwordHash).IsSatisfied(),
            new EmailMustBeValid(email).IsSatisfied(),
            new UserMustHaveAtLeastOneRole(roleNames).IsSatisfied(),
            new UserMustHaveValidMfaState(mfaSecret, isMfaEnabled, recoveryCodes).IsSatisfied()
        };
            
        var results = validationResults.Where(result => result.IsFailure);

        return results.ToArray();
    }
}