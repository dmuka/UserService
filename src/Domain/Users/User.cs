using Core;
using Domain.Permissions;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.ValueObjects;

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
    public static User CreateUser(
        Guid userId,
        string userName,
        string firstName,
        string lastName,
        PasswordHash passwordHash, 
        Email email, 
        ICollection<RoleId> roleIds,
        ICollection<UserPermissionId> userPermissionIds)
    {
        return new User(
            new UserId(userId), 
            userName, 
            firstName, 
            lastName, 
            passwordHash, 
            email, 
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
        ValidateUserDetails(userName, firstName, lastName, passwordHash, email, roleIds, userPermissionIds);

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
    public void ChangeEmail(Email newEmail)
    {
        ArgumentNullException.ThrowIfNull(newEmail, nameof(newEmail));
        Email = newEmail;
    }

    /// <summary>
    /// Changes the password hash of the user.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    public void ChangePassword(PasswordHash newPasswordHash)
    {
        ArgumentNullException.ThrowIfNull(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Removes the role of the user.
    /// </summary>
    /// <param name="roleId">The role id to remove.</param>
    public void RemoveRole(RoleId roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));
        
        if (RoleIds.Count == 1)
            throw new ArgumentException("User must have at least one role.", nameof(roleId));
        
        RoleIds.Remove(roleId);
    }

    /// <summary>
    /// Removes the permission of the user.
    /// </summary>
    /// <param name="userPermissionId">The permission to remove.</param>
    public void RemovePermission(UserPermissionId userPermissionId)
    {
        ArgumentNullException.ThrowIfNull(userPermissionId, nameof(userPermissionId));
        
        UserPermissionIds.Remove(userPermissionId);
    }

    /// <summary>
    /// Adds the role of the user.
    /// </summary>
    /// <param name="roleId">The role id to add.</param>
    public void AddRole(RoleId roleId) => RoleIds.Add(roleId);

    /// <summary>
    /// Adds the permission to the user.
    /// </summary>
    /// <param name="userPermissionId">The permission id to add.</param>
    public void AddPermission(UserPermissionId userPermissionId) => UserPermissionIds.Add(userPermissionId);

    /// <summary>
    /// Validates user details.
    /// </summary>
    private static void ValidateUserDetails(
        string userName,
        string firstName,
        string lastName,
        PasswordHash passwordHash,
        Email email,
        ICollection<RoleId> roleIds,
        ICollection<UserPermissionId> userPermissionIds)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username can't be null or empty.", nameof(userName));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name can't be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name can't be null or empty.", nameof(lastName));
        ArgumentNullException.ThrowIfNull(passwordHash, nameof(passwordHash));
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        ArgumentNullException.ThrowIfNull(roleIds, nameof(roleIds));
        if (roleIds.Count == 0)
            throw new ArgumentException("User must have at least one role.", nameof(roleIds));
        ArgumentNullException.ThrowIfNull(userPermissionIds, nameof(userPermissionIds));
    }
}