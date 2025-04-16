using Npgsql;

namespace Domain.Roles;

/// <summary>
/// Interface for user role repository operations.
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// Retrieves a list of user IDs associated with a specific role ID.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of user IDs.</returns>
    Task<IList<Guid>> GetUsersIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of role IDs associated with a specific user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of role IDs.</returns>
    Task<IList<Guid>> GetRolesIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the roles associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="rolesIds">A collection of role IDs to associate with the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of roles updated.</returns>
    Task<int> UpdateUserRolesAsync(Guid userId, IEnumerable<Guid> rolesIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all roles from a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of roles removed.</returns>
    Task<int> RemoveAllUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}