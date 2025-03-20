using Application.Abstractions.Authentication;
using Application.Roles.Add;
using Application.Roles.AddRole;
using Domain.Roles;
using Moq;

namespace UserService.Application.Tests.Roles.Add;

[TestFixture]
public class AddRoleCommandHandlerTests
{
    private const string AdminRoleName = "AdminRoleName";
    private const string UserRoleName = "UserRoleName";
    
    private readonly Guid _roleId = Guid.CreateVersion7();
        
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private AddRoleCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new AddRoleCommandHandler(_roleRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_RoleNameAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var command = new AddRoleCommand(AdminRoleName);
        _roleRepositoryMock.Setup(repository => repository.IsRoleNameExistsAsync(AdminRoleName, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.RoleNameAlreadyExists));
        }
    }

    [Test]
    public async Task Handle_NewRoleAdded_ReturnsSuccess()
    {
        // Arrange
        var command = new AddRoleCommand(UserRoleName);
        _roleRepositoryMock.Setup(repository => repository.IsRoleNameExistsAsync(UserRoleName, _cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _roleRepositoryMock.Verify(repo => repo.AddRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}