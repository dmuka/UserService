using Core;
using Domain.Users;

namespace Domain.Roles;

public class Role : Entity, IAggregationRoot
{
    public RoleId Id { get; private set; }
    public string Name { get; private set; }
    public ICollection<UserId> UserIds { get; private set; }

    protected Role() { }

    public static Role CreateRole(Guid id, string name)
    {
        return new Role(new RoleId(id), name);
    }

    private Role(RoleId id, string name)
    {
        Id = id;
        Name = name;
        UserIds = new List<UserId>();
    }
        
    public void AddUser(UserId userId)
    {
        UserIds.Add(userId);
    }
}