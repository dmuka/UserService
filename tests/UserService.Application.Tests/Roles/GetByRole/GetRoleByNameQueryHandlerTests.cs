using Application.Abstractions.Authentication;
using Application.Roles.GetByName;
using Domain.Roles;
using Moq;

namespace UserService.Application.Tests.Roles.GetByRole;

[TestFixture]
public class GetRoleByNameQueryHandlerTests
{
    private readonly Role _role = Role.CreateRole(Guid.CreateVersion7(), "Admin");
    private const string NonExistentRoleName = "NonExistentRole";
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
 
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetRoleByNameQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetRoleByNameQueryHandler(_roleRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_RoleExists_ReturnsRoleResponse()
    {
        // Arrange
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(_role.Name, _cancellationToken))
            .ReturnsAsync(_role);

        // Act
        var result = await _handler.Handle(new GetRoleByNameQuery(_role.Name), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Name, Is.EqualTo(_role.Name));
        }
    }

    [Test]
    public async Task Handle_RoleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync(NonExistentRoleName, _cancellationToken))
            .ReturnsAsync((Role)null!);

        // Act
        var result = await _handler.Handle(new GetRoleByNameQuery(NonExistentRoleName), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.NotFound(NonExistentRoleName)));
        }
    }
}