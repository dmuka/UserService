using Domain.Users;

namespace Domain.Roles;

public interface IRoleRepository
{
    Task<bool> IsRoleNameExistsAsync(string roleName, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<int> RemoveRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IList<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IList<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<Guid> AddRoleAsync(Role role, CancellationToken cancellationToken = default);
    Task<int> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default);
}