using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;

namespace UserService.Domain.Tests.Aggregates;

[TestFixture]
public class RoleTests
{
    private readonly Guid _id = Guid.CreateVersion7();
    
    [Test]
    public void CreateRole_ShouldInitializeProperties()
    {
        // Arrange
        const string roleName = "Administrator";

        // Act
        var role = Role.CreateRole(_id, roleName);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(role.Id, Is.EqualTo(new RoleId(_id)));
            Assert.That(role.Name, Is.EqualTo(roleName));
            Assert.That(role.Users, Is.Empty);
        }
    }

    [Test]
    public void AddUser_ShouldAddUserToRole()
    {
        // Arrange
        var role = Role.CreateRole(_id, "User");
        var user = User.CreateUser(
            _id, 
            "username", 
            "firstName", 
            "lastName", 
            new PasswordHash("hash"), 
            new Email("email@email.com"), 
            new List<Role>()); 

        // Act
        role.AddUser(user);

        // Assert
        Assert.That(role.Users, Contains.Item(user));
        Assert.That(role.Users, Has.Count.EqualTo(1));
    }
}