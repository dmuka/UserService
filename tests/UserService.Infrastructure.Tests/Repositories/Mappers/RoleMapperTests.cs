using Domain.Roles;
using Infrastructure.Repositories.Dtos;
using Infrastructure.Repositories.Mappers;

namespace UserService.Infrastructure.Tests.Repositories.Mappers;

[TestFixture]
public class RoleMapperTests
{
    private RoleMapper _roleMapper;

    [SetUp]
    public void Setup()
    {
        _roleMapper = new RoleMapper();
    }

    [Test]
    public void ToDto_ShouldMapRoleToRoleDtoCorrectly()
    {
        // Arrange
        var roleId = new RoleId(Guid.CreateVersion7());
        var role = new Role(roleId, "Admin");

        // Act
        var roleDto = _roleMapper.ToDto(role);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(roleDto.Id, Is.EqualTo(roleId.Value));
            Assert.That(roleDto.Name, Is.EqualTo(role.Name));
        }
    }

    [Test]
    public void ToEntity_ShouldMapRoleDtoToRoleCorrectly()
    {
        // Arrange
        var roleDto = new RoleDto { Id = Guid.CreateVersion7(), Name = "Admin" };

        // Act
        var role = _roleMapper.ToEntity(roleDto);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(role.Id.Value, Is.EqualTo(roleDto.Id));
            Assert.That(role.Name, Is.EqualTo(roleDto.Name));
        }
    }

    [Test]
    public void ToEntity_ShouldThrowArgumentNullException_WhenRoleDtoIsNull()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _roleMapper.ToEntity(null));
    }
}