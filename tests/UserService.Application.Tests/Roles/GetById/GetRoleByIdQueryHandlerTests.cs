using Application.Abstractions.Authentication;
using Application.Roles.GetById;
using Domain.Roles;
using Domain.Users;
using Moq;

namespace UserService.Application.Tests.Roles.GetById;

[TestFixture]
public class GetRoleByIdQueryHandlerTests
{
    private readonly Role _role = Role.CreateRole(Guid.CreateVersion7(), "Admin");
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetRoleByIdQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetRoleByIdQueryHandler(_roleRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_RoleExists_ReturnsRoleResponse()
    {
        // Arrange
        _roleRepositoryMock.Setup(x => x.GetRoleByIdAsync(_role.Id.Value, _cancellationToken))
            .ReturnsAsync(_role);

        // Act
        var result = await _handler.Handle(new GetRoleByIdQuery(_role.Id.Value), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo(_role.Id.Value));
        }
    }

    [Test]
    public async Task Handle_RoleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _roleRepositoryMock.Setup(x => 
            x.GetRoleByIdAsync(_role.Id.Value, _cancellationToken)).ReturnsAsync((Role)null);

        // Act
        var result = await _handler.Handle(new GetRoleByIdQuery(_role.Id.Value), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.NotFound(_role.Id.Value)));
        }
    }
}