namespace Infrastructure.Repositories.Dtos;

public record UserDto : IDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public string[]? RecoveryCodesHashes { get; set; }
}