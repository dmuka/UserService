using Application.Abstractions.Authentication;
using Application.Users.GetById;
using Domain.Users;
using Domain.ValueObjects;
using Moq;
using Shouldly;

namespace UserService.Application.Tests.Users.GetById;

[TestFixture]
public class GetUserByIdQueryHandlerTests
{
    private const ulong AuthorizedUserId = 123;
    private const ulong UnauthorizedUserId = 456;
    
    private Mock<IUserRepository> _mockRepository;
    private Mock<IUserContext> _mockUserContext;
    private GetUserByIdQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockUserContext = new Mock<IUserContext>();
        _handler = new GetUserByIdQueryHandler(_mockRepository.Object, _mockUserContext.Object);
    }

    [Test]
    public async Task Handle_UserMismatch_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetUserByIdQuery(AuthorizedUserId);
        _mockUserContext.Setup(x => x.UserId).Returns(UnauthorizedUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("Users.Unauthorized"));
        }
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var query = new GetUserByIdQuery(AuthorizedUserId);
        _mockUserContext.Setup(x => x.UserId).Returns(AuthorizedUserId);
        _mockRepository.Setup(x => x.GetUserByIdAsync(AuthorizedUserId, CancellationToken.None)).ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("UserNotFound"));
            Assert.That(result.Error.Description, Is.EqualTo($"The user with the id = '{AuthorizedUserId}' was not found."));
        }
    }

    [Test]
    public async Task Handle_UserFound_ReturnsUserResponse()
    {
        // Arrange
        var query = new GetUserByIdQuery(AuthorizedUserId);
        _mockUserContext.Setup(x => x.UserId).Returns(AuthorizedUserId);
        var user = new User(
            "userName", 
            "John",
            "Doe", 
            new PasswordHash("hash"), 
            new Email("email@email.com"), 
            new Role("Admin"));
        user.SetId(AuthorizedUserId);

        var expected = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            RoleId = user.RoleId
        };
        
        _mockRepository.Setup(x => x.GetUserByIdAsync(AuthorizedUserId, CancellationToken.None)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            result.Value.ShouldBeEquivalentTo(expected);
        }
    }
}