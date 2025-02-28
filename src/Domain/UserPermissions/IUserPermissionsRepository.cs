using Domain.Users;

namespace Domain.UserPermissions;

public interface IUserPermissionsRepository
{
    Task<IList<UserPermission>> GetPermissionsByUserAsync(UserId userId, CancellationToken cancellationToken = default);
}