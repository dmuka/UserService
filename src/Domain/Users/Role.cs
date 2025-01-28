namespace Domain.Users;

public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public ICollection<User> Users { get; private set; }

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