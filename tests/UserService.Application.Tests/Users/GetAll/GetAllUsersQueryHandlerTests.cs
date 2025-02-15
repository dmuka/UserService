using Application.Abstractions.Authentication;
using Application.Users.GetAll;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Moq;

namespace UserService.Application.Tests.Users.GetAll;

[TestFixture]
public class GetAllUsersQueryHandlerTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetAllUsersQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_UserIsNotAuthorized_ReturnsUnauthorizedResult()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserRole).Returns("Admin");

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.Unauthorized()));
        }
    }

    [Test]
    public async Task Handle_UserIsAuthorized_ReturnsUsers()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserRole).Returns("User");
        var users = new List<User>
        {
            User.CreateUser(Guid.CreateVersion7(), "name", "First Name", "Last Name", new PasswordHash("hash"), new Email("email@email.com"), new List<Role>()),
            User.CreateUser(Guid.CreateVersion7(), "name", "First Name", "Last Name", new PasswordHash("hash"), new Email("email@email.com"), new List<Role>())
        };
        _userRepositoryMock.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count(), Is.EqualTo(users.Count));
        }
    }
}