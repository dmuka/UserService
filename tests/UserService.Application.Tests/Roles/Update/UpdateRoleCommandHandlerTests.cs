using Application.Abstractions.Authentication;
using Application.Roles.Update;
using Core;
using Domain.Roles;
using Moq;

namespace UserService.Application.Tests.Roles.Update;

[TestFixture]
public class UpdateRoleCommandHandlerTests
{
    private readonly Guid _adminRoleId = Guid.CreateVersion7();
    private const string AdminRoleName = "Admin";
    private const string UserRoleName = "User";
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private UpdateRoleCommandHandler _handler;
    private Role _role;

    [SetUp]
    public void SetUp()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new UpdateRoleCommandHandler(_roleRepositoryMock.Object, _userContextMock.Object);
        
        _role = Role.Create(
            _adminRoleId,
            AdminRoleName).Value;
    }

    [Test]
    public async Task Handle_UserIsNotAdmin_ReturnsUnauthorizedResult()
    {
        // Arrange
        var command = new UpdateRoleCommand(_role);
        _userContextMock.Setup(x => x.UserRole).Returns(UserRoleName);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.Unauthorized()));
        }
    }

    [Test]
    public async Task Handle_UserIsAdmin_UpdatesRoleSuccessfully()
    {
        // Arrange
        var command = new UpdateRoleCommand(_role);
        _userContextMock.Setup(x => x.UserRole).Returns(AdminRoleName);

        _roleRepositoryMock.Setup(x => x.UpdateRoleAsync(_role, _cancellationToken))
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