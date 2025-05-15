using Core;

namespace Domain.Users;

public class UserId(Guid value) : TypedId(value)
{
    /// <summary>
    /// Explicitly converts a guid to a <see cref="UserId"/>.
    /// </summary>
    /// <param name="userId">The guid to convert.</param>
    public static explicit operator UserId(Guid userId) => new (userId);
}