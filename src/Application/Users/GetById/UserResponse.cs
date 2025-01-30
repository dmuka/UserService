using Domain.Users;

namespace Application.Users.GetById;

public sealed record UserResponse
{
    public required ulong Id { get; init; }    
    public required string Username { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string PasswordHash { get; init; }
    public required string Email { get; init; }
    public required ulong RoleId { get; init; }

    public static UserResponse Create(User user)
    {
        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PasswordHash = user.PasswordHash,
            Email = user.Email,
            RoleId = user.RoleId
        };
        
        return userResponse;
    }
}
