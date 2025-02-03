using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Repositories.Dtos;
using Infrastructure.Repositories.Mappers;

namespace UserService.Infrastructure.Tests.Repositories.Mappers
{
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
            var userId = new UserId(1);
            var role = new Role(new RoleId(1), "Admin");
            var user = new User(userId, "jdoe", "John", "Doe", new PasswordHash("hashedPassword"), new Email("jdoe@example.com"), role);

            // Act
            var userDto = _userMapper.ToDto(user);

            // Assert
            Assert.That(userDto.Id, Is.EqualTo(userId.Value));
            Assert.That(userDto.Username, Is.EqualTo(user.Username));
            Assert.That(userDto.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(userDto.LastName, Is.EqualTo(user.LastName));
            Assert.That(userDto.PasswordHash, Is.EqualTo(user.PasswordHash.Value));
            Assert.That(userDto.Email, Is.EqualTo(user.Email.Value));
            Assert.That(userDto.RoleId, Is.EqualTo(role.Id.Value));
            Assert.That(userDto.RoleName, Is.EqualTo(role.Name));
        }

        [Test]
        public void ToEntity_ShouldMapUserDtoToUserCorrectly()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 1,
                Username = "jdoe",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hashedPassword",
                Email = "jdoe@example.com",
                RoleId = 1,
                RoleName = "Admin"
                    
            };

            // Act
            var user = _userMapper.ToEntity(userDto);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(user.Id.Value, Is.EqualTo(userDto.Id));
                Assert.That(user.Username, Is.EqualTo(userDto.Username));
                Assert.That(user.FirstName, Is.EqualTo(userDto.FirstName));
                Assert.That(user.LastName, Is.EqualTo(userDto.LastName));
                Assert.That(user.PasswordHash.Value, Is.EqualTo(userDto.PasswordHash));
                Assert.That(user.Email.Value, Is.EqualTo(userDto.Email));
                Assert.That(user.Role.Id.Value, Is.EqualTo(userDto.RoleId));
                Assert.That(user.Role.Name, Is.EqualTo(userDto.RoleName));
            }
        }

        [Test]
        public void ToEntity_ShouldThrowArgumentNullException_WhenUserDtoIsNull()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _userMapper.ToEntity(null));
        }
    }
}