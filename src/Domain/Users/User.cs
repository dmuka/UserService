using Domain.ValueObjects;

namespace Domain.Users;

public class User : Entity
{
    public int Id { get; set; }
    public string Username { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public Email Email { get; private set; }
    public int RoleId { get; private set; }
    public Role Role { get; private set; }

    public User() { }

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

        Username = userName;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Role = role ?? throw new ArgumentNullException(nameof(role));
        RoleId = role.Id;
    }

    public void ChangeEmail(Email newEmail)
    {
        ArgumentNullException.ThrowIfNull(newEmail);

        Email = newEmail;
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        ArgumentNullException.ThrowIfNull(newPasswordHash);

        PasswordHash = newPasswordHash;
    }

    public void ChangeRole(Role newRole)
    {
        ArgumentNullException.ThrowIfNull(newRole);

        Role = newRole;
        RoleId = newRole.Id;
    }
}