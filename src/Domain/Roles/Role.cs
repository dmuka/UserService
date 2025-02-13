using Core;
using Domain.Users;

namespace Domain.Roles;

public class Role : Entity, IAggregationRoot
{
    public RoleId Id { get; private set; }
    public string Name { get; private set; }
    public ICollection<User> Users { get; private set; }

    protected Role() { }

    public Role(RoleId id, string name)
    {
        Id = id;
        Name = name;
        Users = new List<User>();
    }
        
    public void AddUser(User user)
    {
        Users.Add(user);
    }
}