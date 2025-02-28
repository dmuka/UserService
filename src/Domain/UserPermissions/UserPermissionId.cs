using Core;

namespace Domain.UserPermissions;

public class UserPermissionId(Guid userPermissionId) : TypedId(userPermissionId);