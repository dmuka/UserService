using Application.Abstractions.Authentication;
using Application.Users.Update;
using Domain.Users;
using Domain.ValueObjects;
using Moq;
using NUnit.Framework;
using Core;
using Domain.Roles;

namespace UserService.Application.Tests.Users.Update;

[TestFixture]
public class UpdateUserCommandHandlerTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private UpdateUserCommandHandler _handler;
    private User _user;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _userRoleRepositoryMock.Object, _userContextMock.Object);
        
        _user = User.Create(
            Guid.NewGuid(),
            "username",
            "FirstName",
            "LastName",
            "hash",
            "email@example.com",
            new List<RoleId> { new(Guid.CreateVersion7()) },
            new List<Domain.UserPermissions.UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;
    }

    [Test]
    public async Task Handle_UserIsNotAdmin_ReturnsUnauthorizedResult()
    {
        // Arrange
        var command = new UpdateUserCommand(_user);
        _userContextMock.Setup(x => x.UserRole).Returns("User");

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.Unauthorized()));
        }
    }

    [Test]
    public async Task Handle_UserIsAdmin_UpdatesUserSuccessfully()
    {
        // Arrange
        var command = new UpdateUserCommand(_user);
        _userContextMock.Setup(x => x.UserRole).Returns("Admin");

        _userRepositoryMock.Setup(x => x.UpdateUserAsync(_user, _cancellationToken))
            .Returns(Task.CompletedTask);
        
        _userRoleRepositoryMock.Setup(x => x.UpdateUserRolesAsync(_user.Id.Value, It.IsAny<IEnumerable<Guid>>(), _cancellationToken))
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