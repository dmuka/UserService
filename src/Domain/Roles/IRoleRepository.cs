using Domain.Users;

namespace Domain.Roles;

public interface IRoleRepository
{
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<Guid> AddRoleAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(Role role, CancellationToken cancellationToken = default);
}