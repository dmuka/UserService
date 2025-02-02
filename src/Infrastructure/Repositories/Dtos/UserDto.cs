namespace Infrastructure.Repositories.Dtos;

public record UserDto(
    long Id,
    string Username,
    string FirstName,
    string LastName,
    string PasswordHash,
    string Email,
    long RoleId) : IDto;