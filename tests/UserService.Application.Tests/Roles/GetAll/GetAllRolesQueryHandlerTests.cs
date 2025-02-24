using Application.Abstractions.Authentication;
using Application.Roles.GetAll;
using Domain.Roles;
using Moq;

namespace UserService.Application.Tests.Roles.GetAll;

[TestFixture]
public class GetAllRolesQueryHandlerTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetAllRolesQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_UserIsNotAdmin_ReturnsUnauthorized()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserRole).Returns("User");

        // Act
        var result = await _handler.Handle(new GetAllRolesQuery(), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(RoleErrors.Unauthorized()));
        }
    }

    [Test]
    public async Task Handle_UserIsAdmin_ReturnsRoles()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserRole).Returns("Admin");
        var roles = new List<Role>
        {
            Role.CreateRole(Guid.CreateVersion7(), "Admin"), 
            Role.CreateRole(Guid.CreateVersion7(), "User")
        };
        _roleRepositoryMock.Setup(x => x.GetAllRolesAsync(_cancellationToken))
            .ReturnsAsync(roles);

        // Act
        var result = await _handler.Handle(new GetAllRolesQuery(), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count(), Is.EqualTo(roles.Count));
        }
    }
}