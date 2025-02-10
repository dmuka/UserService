using Domain.Users;

namespace Application.Abstractions.Authentication;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public User User { get; set; }
}