namespace Infrastructure.Repositories.Dtos;

public record RoleDto : IDto
{
    public long Id { get; set; }
    public string Name { get; set; }
}