using Application.Abstractions.Authentication;
using Application.Roles.RemoveRole;
using Domain.Roles;
using Moq;

namespace UserService.Application.Tests.Roles.Remove;

[TestFixture]
public class RemoveRoleCommandHandlerTests
{
    private static readonly Guid Id = Guid.CreateVersion7();
    private readonly RemoveRoleCommand _command = new (Id);
        
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private RemoveRoleCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
            
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            
        _userContextMock = new Mock<IUserContext>();
            
        _handler = new RemoveRoleCommandHandler(
            _roleRepositoryMock.Object, 
            _userRoleRepositoryMock.Object, 
            _userContextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnFailure_WhenUsersWithRoleExist()
    {
        // Arrange
        var command = _command;
        _userRoleRepositoryMock.Setup(repository => repository.GetUsersIdsByRoleIdAsync(command.RoleId, _cancellationToken))
            .ReturnsAsync(new List<Guid> { Id });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.UsersWithAssignedRole));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnSuccess_WhenRoleIsRemoved()
    {
        // Arrange
        var command = _command;
        _userRoleRepositoryMock.Setup(repo => repo.GetUsersIdsByRoleIdAsync(command.RoleId, _cancellationToken))
            .ReturnsAsync(new List<Guid>());
        _roleRepositoryMock.Setup(repo => repo.RemoveRoleByIdAsync(command.RoleId, _cancellationToken))
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

    [Test]
    public async Task Handle_ShouldReturnFailure_WhenRoleNotFound()
    {
        // Arrange
        var command = _command;
        _userRoleRepositoryMock.Setup(repo => repo.GetUsersIdsByRoleIdAsync(command.RoleId, _cancellationToken))
            .ReturnsAsync(new List<Guid>());
        _roleRepositoryMock.Setup(repo => repo.RemoveRoleByIdAsync(command.RoleId, _cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.NotFound(command.RoleId)));
        }
    }
}