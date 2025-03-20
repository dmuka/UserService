using Core;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users.DomainEvents;
using Domain.Users.Specifications;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;

namespace Domain.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity, IAggregationRoot
{
    public UserId Id { get; private set; }
    public string Username { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public Email Email { get; private set; }
    public ICollection<RoleId> RoleIds { get; private set; }
    public ICollection<UserPermissionId> UserPermissionIds { get; private set; }

    /// <summary>
    /// Default constructor for ORM compatibility.
    /// </summary>
    protected User() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class with specified user details.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="userName">The username of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <param name="passwordHash">The hashed password of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="roleIds">Collection of the user role ids.</param>
    /// <param name="userPermissionIds">Collection of the user permission ids.</param>
    /// <exception cref="ArgumentException">Thrown when any string parameter is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when any object parameter is null.</exception>
    public static Result<User> CreateUser(
        Guid userId,
        string userName,
        string firstName,
        string lastName,
        string passwordHash, 
        string email, 
        ICollection<RoleId> roleIds,
        ICollection<UserPermissionId> userPermissionIds)
    {
        var resultsWithFailures = ValidateUserDetails(
            userName, 
            firstName, 
            lastName,
            passwordHash,
            email,
            roleIds,
            userPermissionIds);

        if (resultsWithFailures.Length != 0)
        {
            return Result<User>.ValidationFailure(ValidationError.FromResults(resultsWithFailures));
        }

        return new User(
            new UserId(userId), 
            userName, 
            firstName, 
            lastName, 
            PasswordHash.Create(passwordHash), 
            Email.Create(email), 
            roleIds,
            userPermissionIds);
    }    
    
    private User(
        UserId userId,
        string userName,
        string firstName,
        string lastName,
        PasswordHash passwordHash, 
        Email email, 
        ICollection<RoleId> roleIds,
        ICollection<UserPermissionId> userPermissionIds)
    {
        Id = userId;
        Username = userName;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Email = email;
        RoleIds = roleIds;
        UserPermissionIds = userPermissionIds;
    }

    /// <summary>
    /// Changes the email address of the user.
    /// </summary>
    /// <param name="newEmail">The new email address.</param>
    public Result ChangeEmail(Email newEmail)
    {
        if (newEmail is null) return Result.Failure<Email>(Error.NullValue);
        
        Email = newEmail;
    
        var emailChangedEvent = new UserEmailChangedEvent(Id.Value, newEmail.Value);
        AddDomainEvent(emailChangedEvent);
    
        return Result.Success();
    }

    /// <summary>
    /// Changes the password hash of the user.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    public Result ChangePassword(PasswordHash newPasswordHash)
    {
        if (newPasswordHash is null) return Result.Failure<PasswordHash>(Error.NullValue);
        
        PasswordHash = newPasswordHash;
        
        return Result.Success();
    }

    /// <summary>
    /// Removes the role of the user.
    /// </summary>
    /// <param name="roleId">The role id to remove.</param>
    public Result RemoveRole(RoleId roleId)
    {
        if (roleId is null) return Result.Failure<RoleId>(Error.NullValue);
        var validationResult = new UserMustHaveAtLeastOneRoleAfterRemoveRole(RoleIds).IsSatisfied();
        if (validationResult.IsFailure) return validationResult;
        
        RoleIds.Remove(roleId);
        
        return Result.Success();
    }

    /// <summary>
    /// Removes the permission of the user.
    /// </summary>
    /// <param name="userPermissionId">The permission to remove.</param>
    public Result RemovePermission(UserPermissionId userPermissionId)
    {
        if (userPermissionId is null) return Result.Failure<UserPermissionId>(Error.NullValue);  
        
        UserPermissionIds.Remove(userPermissionId);
        
        return Result.Success();
    }

    /// <summary>
    /// Adds the role of the user.
    /// </summary>
    /// <param name="roleId">The role id to add.</param>
    public Result AddRole(RoleId roleId)
    {
        if (roleId is null) return Result.Failure<RoleId>(Error.NullValue);
        
        RoleIds.Add(roleId); 
        
        return Result.Success();
    }

    /// <summary>
    /// Adds the permission to the user.
    /// </summary>
    /// <param name="userPermissionId">The permission id to add.</param>
    public void AddPermission(UserPermissionId userPermissionId) => UserPermissionIds.Add(userPermissionId);

    /// <summary>
    /// Validates user details.
    /// </summary>
    private static Result[] ValidateUserDetails(
        string userName,
        string firstName,
        string lastName,
        string passwordHash,
        string email,
        ICollection<RoleId> roleIds,
        ICollection<UserPermissionId> userPermissionIds)
    {
        var validationResults = new []
        {
            new UserNameMustBeValid(userName).IsSatisfied(),
            new FirstNameMustBeValid(firstName).IsSatisfied(),
            new LastNameMustBeValid(lastName).IsSatisfied(),
            new MustBeNonNullValue<string>(passwordHash).IsSatisfied(),
            new EmailMustBeValid(email).IsSatisfied(),
            new UserMustHaveAtLeastOneRole(roleIds).IsSatisfied(),
            new MustBeNonNullValue<ICollection<UserPermissionId>>(userPermissionIds).IsSatisfied()
        };
            
        var results = validationResults.Where(result => result.IsFailure);

        return results.ToArray();
    }
}