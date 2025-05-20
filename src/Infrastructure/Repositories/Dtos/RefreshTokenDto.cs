namespace Infrastructure.Repositories.Dtos;

public class RefreshTokenDto : IDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime ExpiresUtc { get; set; }
    public Guid UserId { get; set; }
}