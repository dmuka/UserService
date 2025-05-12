namespace Infrastructure.Repositories.Dtos;

public record UserDto : IDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public List<string>? RecoveryCodes { get; set; }
}