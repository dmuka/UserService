using Domain.Users;

namespace Application.Users.GetById;

public sealed record UserResponse
{
    public required Guid Id { get; init; }    
    public required string Username { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string PasswordHash { get; init; }
    public required string Email { get; init; }
    public required (string name, Guid id)[] Roles { get; init; }

    public static UserResponse Create(User user, (string name, Guid id)[] roles)
    {
        var userResponse = new UserResponse
        {
            Id = user.Id.Value,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PasswordHash = user.PasswordHash,
            Email = user.Email,
            Roles = roles
        };
        
        return userResponse;
    }
}
