namespace Infrastructure.Repositories.Dtos;

public record RoleDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}