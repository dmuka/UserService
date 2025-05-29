using Application.Abstractions.Authentication;
using Application.Users.GenerateMfaArtifacts;
using Application.Users.ResetPassword;
using Domain.UserPermissions;
using Domain.Users;
using Domain.Users.Constants;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;
using Moq;
using Shouldly;

namespace UserService.Application.Tests.Users.ResetPassword;

[TestFixture]
public class ResetPasswordCommandHandlerTests
{
    private const string Admin = "Admin";
    private const string InvalidUserId = "InvalidUserId";
    private const string Password = "Password";
    private const string Hash = "PasswordHash";
    private static readonly RoleName AdminRoleName = RoleName.Create(Admin);
    
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid NonExistentUserId = Guid.CreateVersion7();
    private User _user = 
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
            "Secret",
            true).Value;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IPasswordHasher> _mockPasswordHasher;
    
    private ResetPasswordCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserRepository.Setup(repository => repository.GetUserByIdAsync(UserId, _cancellationToken))
            .ReturnsAsync(_user);
        
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockPasswordHasher.Setup(hasher => hasher.GetHash(Password)).Returns(Hash);

        _handler = new ResetPasswordCommandHandler(_mockUserRepository.Object, _mockPasswordHasher.Object);
    }

    [Test]
    public async Task Handle_WhenAllDataValid_ShouldReturnAsExpected()
    {
        // Arrange
        var command = new ResetPasswordCommand(UserId, Password);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }
        _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WhenUserIdNonExistent_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand(NonExistentUserId, Password);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Codes.NotFound));
            _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Never);
        }
    }
}