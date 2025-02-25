using Application.Abstractions.Authentication;
using Application.Users.GetByName;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Moq;

namespace UserService.Application.Tests.Users.GetByName;

[TestFixture]
public class GetUserByNameQueryHandlerTests
{
    private const string ExistingUsername = "existingUser";
    private const string AnotherExistingUsername = "anotherExistingUser";
    private const string NonExistingUsername = "nonExistingUser";
    
    private Mock<IUserRepository> _repositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetUserByNameQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetUserByNameQueryHandler(_repositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNameDoesNotMatch()
    {
        // Arrange
        var query = new GetUserByNameQuery(ExistingUsername);
        _userContextMock.Setup(uc => uc.UserName).Returns(AnotherExistingUsername);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
        _repositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
        var user = User.CreateUser(
            Guid.CreateVersion7(),
            ExistingUsername,
            "firstName",
            "lastName",
            new PasswordHash("hash"),
            new Email("email@email.com"),
            new List<Role>());
        
        _userContextMock.Setup(uc => uc.UserName).Returns(ExistingUsername);
        _repositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        }
    }
}