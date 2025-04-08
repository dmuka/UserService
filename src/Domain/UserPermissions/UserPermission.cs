using Core;
using Domain.Permissions;
using Domain.Users;

namespace Domain.UserPermissions;

public class UserPermission : Entity
{
    public new UserPermissionId Id { get; private set; }
    public UserId UserId { get; private set; }
    public PermissionId PermissionId { get; private set; }

    /// <summary>
    /// Default constructor for ORM compatibility.
    /// </summary>
    protected UserPermission() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPermission"/> class.
    /// </summary>
    /// <param name="userPermissionId">The unique identifier of the user permission.</param>
    /// <param name="userId">The unique identifier of the user who owned the permission.</param>
    /// <param name="permissionId">The unique identifier of the permission of the user.</param>
    /// <exception cref="ArgumentNullException">Thrown when any object parameter is null.</exception>
    public static UserPermission Create(Guid userPermissionId, UserId userId, PermissionId permissionId)
    {
        return new UserPermission(userPermissionId, userId, permissionId);
    }    
    
    private UserPermission(Guid userPermissionId, UserId userId, PermissionId permissionId)
    {
        ValidatePermissionDetails(userId, permissionId);

        Id = new UserPermissionId(userPermissionId);
        UserId = userId;
        PermissionId = permissionId;
    }
    
    /// <summary>
    /// Validates permission details.
    /// </summary>
    private static void ValidatePermissionDetails(UserId userId, PermissionId permissionId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(permissionId, nameof(permissionId));
    }
}