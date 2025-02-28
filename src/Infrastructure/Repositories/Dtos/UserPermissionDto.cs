namespace Infrastructure.Repositories.Dtos;

public class UserPermissionDto : IDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
}