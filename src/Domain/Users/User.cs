using Core;
using Domain.Roles;
using Domain.ValueObjects;

namespace Domain.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity, IAggregationRoot
{
    public string Username { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public Email Email { get; private set; }
    public long RoleId { get; private set; }
    public Role Role { get; private set; }

    /// <summary>
    /// Default constructor for ORM compatibility.
    /// </summary>
    protected User() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <param name="passwordHash">The hashed password of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="role">The role of the user.</param>
    public User(
        string userName,
        string firstName,
        string lastName,
        PasswordHash passwordHash, 
        Email email, 
        Role role)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username can't be null or empty.", nameof(userName));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name can't be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name can't be null or empty.", nameof(lastName));
        ArgumentNullException.ThrowIfNull(passwordHash, nameof(passwordHash));
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        Username = userName;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Email = email;
        Role = role;
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
    /// Changes the role of the user.
    /// </summary>
    /// <param name="newRole">The new role.</param>
    public void ChangeRole(Role newRole)
    {
        ArgumentNullException.ThrowIfNull(newRole, nameof(newRole));

        SetRole(newRole);
    }

    /// <summary>
    /// Sets the role of the user and updates the role ID.
    /// </summary>
    /// <param name="role">The role to set.</param>
    private void SetRole(Role role)
    {
        Role = role;
        RoleId = role.Id;
    }
}