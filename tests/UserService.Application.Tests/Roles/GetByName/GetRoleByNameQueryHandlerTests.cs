using Application.Abstractions.Authentication;
using Application.Roles.GetByName;
using Domain.Roles;
using Moq;

namespace UserService.Application.Tests.Roles.GetByName;

[TestFixture]
public class GetRoleByNameQueryHandlerTests
{
    private GetRoleByNameQueryHandler _handler;
    private Mock<IRoleRepository> _repositoryMock;
    private Mock<IUserContext> _userContextMock;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new GetRoleByNameQueryHandler(
            _repositoryMock.Object, 
            _userContextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnRoleResponse_WhenRoleExists()
    {
        // Arrange
        const string roleName = "Admin";
        var role = Role.Create(Guid.NewGuid(), roleName);
        var query = new GetRoleByNameQuery(roleName);

        _repositoryMock
            .Setup(repo => repo.GetRoleByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Name, Is.EqualTo(roleName));
        });
    }

    [Test]
    public async Task Handle_ShouldReturnFailure_WhenRoleDoesNotExist()
    {
        // Arrange
        const string roleName = "NonExistent";
        var query = new GetRoleByNameQuery(roleName);

        _repositoryMock
            .Setup(repo => repo.GetRoleByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.NotFound(roleName)));
        });
    }
}