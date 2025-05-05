using Domain.Roles;
using Domain.UserPermissions;
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
        var role = Role.Create(_id, roleName).Value;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(role.Id, Is.EqualTo(new RoleId(_id)));
            Assert.That(role.Name, Is.EqualTo(roleName));
            Assert.That(role.UserIds, Is.Empty);
        }
    }

    [Test]
    public void AddUser_ShouldAddUserToRole()
    {
        // Arrange
        var role = Role.Create(_id, "User").Value;
        var userId = new UserId(_id);

        // Act
        role.AddUser(userId);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(role.UserIds.ElementAt(0).Value, Is.EqualTo(_id));
            Assert.That(role.UserIds, Has.Count.EqualTo(1));
        }
    }
}