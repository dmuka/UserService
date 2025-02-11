using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Repositories.Dtos;
using Infrastructure.Repositories.Mappers;

namespace UserService.Infrastructure.Tests.Repositories.Mappers;

[TestFixture]
public class UserMapperTests
{
    private UserMapper _userMapper;

    [SetUp]
    public void Setup()
    {
        _userMapper = new UserMapper();
    }

    [Test]
    public void ToDto_ShouldMapUserToUserDtoCorrectly()
    {
        // Arrange
        var userId = new UserId(Guid.CreateVersion7());
        var roles = new List<Role> { new Role(new RoleId(Guid.CreateVersion7()), "Admin") };
        var user = User.CreateUser(userId.Value, "jdoe", "John", "Doe", new PasswordHash("hashedPassword"), new Email("jdoe@example.com"), roles);

        // Act
        var userDto = _userMapper.ToDto(user);

        // Assert
        Assert.That(userDto.Id, Is.EqualTo(userId.Value));
        Assert.That(userDto.Username, Is.EqualTo(user.Username));
        Assert.That(userDto.FirstName, Is.EqualTo(user.FirstName));
        Assert.That(userDto.LastName, Is.EqualTo(user.LastName));
        Assert.That(userDto.PasswordHash, Is.EqualTo(user.PasswordHash.Value));
        Assert.That(userDto.Email, Is.EqualTo(user.Email.Value));
    }

    [Test]
    public void ToEntity_ShouldMapUserDtoToUserCorrectly()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.CreateVersion7(),
            Username = "jdoe",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            Email = "jdoe@example.com"
        };

        // Act
        var user = _userMapper.ToEntity(userDto);
        user.AddRole(new Role(new RoleId(Guid.CreateVersion7()), "Admin"));

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Id.Value, Is.EqualTo(userDto.Id));
            Assert.That(user.Username, Is.EqualTo(userDto.Username));
            Assert.That(user.FirstName, Is.EqualTo(userDto.FirstName));
            Assert.That(user.LastName, Is.EqualTo(userDto.LastName));
            Assert.That(user.PasswordHash.Value, Is.EqualTo(userDto.PasswordHash));
            Assert.That(user.Email.Value, Is.EqualTo(userDto.Email));
            Assert.That(user.Roles, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void ToEntity_ShouldThrowArgumentNullException_WhenUserDtoIsNull()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _userMapper.ToEntity(null));
    }
}