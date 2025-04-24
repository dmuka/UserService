using Application.Abstractions.Authentication;
using Application.Users.Remove;
using Domain.Users;
using Moq;
using Domain.Roles;

namespace UserService.Application.Tests.Users.Remove;

[TestFixture]
public class RemoveUserCommandHandlerTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private RemoveUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new RemoveUserCommandHandler(_userRepositoryMock.Object, _userRoleRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RemoveUserCommand(userId);

        _userRoleRepositoryMock.Setup(repo => repo.RemoveAllUserRolesAsync(userId, _cancellationToken))
            .ReturnsAsync(0);
        _userRepositoryMock.Setup(repo => repo.RemoveUserByIdAsync(userId, _cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.NotFound(userId)));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnSuccess_WhenUserExistsAndRemoved()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RemoveUserCommand(userId);

        _userRoleRepositoryMock.Setup(repo => repo.RemoveAllUserRolesAsync(userId, _cancellationToken))
            .ReturnsAsync(1);
        _userRepositoryMock.Setup(repo => repo.RemoveUserByIdAsync(userId, _cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(1));
        }
    }
}