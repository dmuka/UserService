namespace Domain.Users;

public interface IUserRepository
{
    Task<bool> IsUsernameExistsAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<Guid> AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
}