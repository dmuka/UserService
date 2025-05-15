using Core;

namespace Domain.Roles;

public class RoleId(Guid value) : TypedId(value)
{
    /// <summary>
    /// Explicitly converts a guid to a <see cref="RoleId"/>.
    /// </summary>
    /// <param name="roleId">The guid to convert.</param>
    public static explicit operator RoleId(Guid roleId) => new (roleId);
}