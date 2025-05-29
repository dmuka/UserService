using Application.Abstractions.Authentication;
using Application.Users.ConfirmEmail;
using Application.Users.EnableMfa;
using Domain.UserPermissions;
using Domain.Users;
using Domain.Users.Constants;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;
using Moq;

namespace UserService.Application.Tests.Users.EnableMfa;

[TestFixture]
public class EnableMfaCommandHandlerTests
{
    private const string Admin = "Admin";
    private const string InvalidUserId = "InvalidUserId";
    private const int VerificationCode = 111111;
    private const int InvalidVerificationCode = 0;
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
    private Mock<ITotpProvider> _mockTotpProvider;
    
    private EnableMfaCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserRepository.Setup(repository => repository.GetUserByIdAsync(UserId, _cancellationToken))
            .ReturnsAsync(_user);
        
        _mockTotpProvider = new Mock<ITotpProvider>();
        _mockTotpProvider.Setup(provider => provider.ValidateTotp(_user.MfaSecret, VerificationCode))
            .Returns(true);

        _handler = new EnableMfaCommandHandler(_mockUserRepository.Object, _mockTotpProvider.Object);
    }

    [Test]
    public async Task Handle_WhenAllDataValid_ShouldReturnAsExpected()
    {
        // Arrange
        var command = new EnableMfaCommand(UserId.ToString(), VerificationCode);
        
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
        var command = new EnableMfaCommand(InvalidUserId, VerificationCode);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Codes.InvalidUserId));
            _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Never);
        }
    }

    [Test]
    public async Task Handle_WhenUserIdNonExistent_ShouldReturnFailure()
    {
        // Arrange
        var command = new EnableMfaCommand(NonExistentUserId.ToString(), VerificationCode);
        
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

    [Test]
    public async Task Handle_WhenVerificationCodeInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new EnableMfaCommand(UserId.ToString(), InvalidVerificationCode);
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Codes.InvalidVerificationCode));
            _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Never);
        }
    }
}