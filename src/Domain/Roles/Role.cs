using Core;
using Domain.Users;

namespace Domain.Roles;

public class Role : Entity
{
    public string Name { get; private set; }
    public ICollection<User> Users { get; private set; }

    protected Role() { }

    public Role(string name)
    {
        Name = name;
        Users = new List<User>();
    }
        
    public void AddUser(User user)
    {
        Users.Add(user);
    }
}