using Application.Users.ConfirmEmail;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;
using Infrastructure.Repositories;
using Moq;

namespace UserService.Application.Tests.Users.ConfirmEmail;

[TestFixture]
public class ConfirmEmailCommandHandlerTests
{
    private const string Admin = "Admin";
    private const string InvalidUserId = "InvalidUserId";
    
    private static readonly RoleName AdminRoleName = RoleName.Create(Admin);
    
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid NonExistentUserId = Guid.CreateVersion7();
    private readonly User _user = 
        User.Create(
            UserId, 
            "name", 
            "First Name", 
            "Last Name", 
            PasswordHash.Create("hash").Value, 
            Email.Create("email@email.com").Value, 
            new List<RoleName> { AdminRoleName }, 
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IUserRepository> _mockUserRepository;
    
    private ConfirmEmailCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserRepository.Setup(repository => repository.GetUserByIdAsync(UserId, _cancellationToken))
            .ReturnsAsync(_user);

        _handler = new ConfirmEmailCommandHandler(_mockUserRepository.Object);
    }

    [Test]
    public async Task Handle_WhenAllDataValid_ShouldReturnAsExpected()
    {
        // Arrange
        var command = new ConfirmEmailCommand(UserId.ToString());
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WhenUserIdInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new ConfirmEmailCommand(InvalidUserId);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("InvalidUserId"));
            _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Never);
        }
    }

    [Test]
    public async Task Handle_WhenUserIdNonExistent_ShouldReturnFailure()
    {
        // Arrange
        var command = new ConfirmEmailCommand(NonExistentUserId.ToString());
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("UserNotFound"));
            _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Never);
        }
    }
}