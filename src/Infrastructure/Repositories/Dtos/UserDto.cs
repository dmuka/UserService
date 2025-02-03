namespace Infrastructure.Repositories.Dtos;

public record UserDto : IDto
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public long RoleId { get; set; }
    public string RoleName  { get; set; }
}