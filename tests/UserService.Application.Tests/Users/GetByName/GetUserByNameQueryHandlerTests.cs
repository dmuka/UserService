using Application.Abstractions.Authentication;
using Application.Users.GetByName;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Moq;

namespace UserService.Application.Tests.Users.GetByName;

[TestFixture]
public class GetUserByNameQueryHandlerTests
{
    private const string ExistingUsername = "existingUser";
    private const string AnotherExistingUsername = "anotherExistingUser";
    private const string NonExistingUsername = "nonExistingUser";
    
    private IList<Role> _roles;
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IUserRepository> _repositoryMock;
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetUserByNameQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roles = new List<Role> { Role.Create(Guid.CreateVersion7(), "Role") };
        
        _repositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetUserByNameQueryHandler(_repositoryMock.Object, _roleRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNameDoesNotMatch()
    {
        // Arrange
        var query = new GetUserByNameQuery(ExistingUsername);
        _userContextMock.Setup(uc => uc.UserName).Returns(AnotherExistingUsername);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.Unauthorized()));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var query = new GetUserByNameQuery(NonExistingUsername);
        _userContextMock.Setup(uc => uc.UserName).Returns(NonExistingUsername);
        _repositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.NotFoundByUsername(query.UserName)));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnUserResponse_WhenUserExists()
    {
        // Arrange
        var query = new GetUserByNameQuery(ExistingUsername);
        var user = User.Create(
            Guid.CreateVersion7(),
            ExistingUsername,
            "firstName",
            "lastName",
            "hash",
            "email@email.com",
            _roles.Select(role => role.Id).ToList(),
            new List<UserPermissionId>()).Value;
        
        _userContextMock.Setup(uc => uc.UserName).Returns(ExistingUsername);
        _repositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync(user);
        _roleRepositoryMock.Setup(repository => repository.GetRolesByUserIdAsync(user.Id.Value, _cancellationToken))
            .ReturnsAsync(_roles);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        }
    }
}