using Application.Abstractions.Authentication;
using Application.Users.GenerateMfaArtifacts;
using Domain.UserPermissions;
using Domain.Users;
using Domain.Users.Constants;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.MfaSecrets;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;
using Moq;
using Shouldly;

namespace UserService.Application.Tests.Users.GenerateMfaArtifacts;

[TestFixture]
public class GenerateMfaArtifactsCommandHandlerTests
{
    private const string Admin = "Admin";
    private const string InvalidUserId = "InvalidUserId";
    private const string Qr = "Qr";
    private const string Secret = "MfaSecret";
    private const int RecoveryCodesCount = 8;
    private readonly List<string> _codes = ["code1", "code2", "code3", "code4", "code5", "code6", "code7", "code8"];
    private readonly List<string> _hashCodes = ["HashCode1", "HashCode2", "HashCode3", "HashCode4", "HashCode5", "HashCode6", "HashCode7", "HashCode8"];
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
            Secret,
            true).Value;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<ITotpProvider> _mockTotpProvider;
    private Mock<IRecoveryCodesProvider> _mockRecoveryCodesProvider;
    
    private GenerateMfaArtifactsCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserRepository.Setup(repository => repository.GetUserByIdAsync(UserId, _cancellationToken))
            .ReturnsAsync(_user);
        
        _mockTotpProvider = new Mock<ITotpProvider>();
        _mockTotpProvider.Setup(provider => provider.GenerateSecretKey()).Returns("Secret");
        _mockTotpProvider.Setup(provider => provider.GetQr(It.IsAny<string>(), _user.Email, It.IsAny<string>()))
            .Returns(Qr);
        
        _mockRecoveryCodesProvider = new Mock<IRecoveryCodesProvider>();
        _mockRecoveryCodesProvider.Setup(provider => provider.GenerateRecoveryCodes(RecoveryCodesCount))
            .Returns([
                (_codes[0], _hashCodes[0]), 
                (_codes[1], _hashCodes[1]), 
                (_codes[2], _hashCodes[2]),
                (_codes[3], _hashCodes[3]),
                (_codes[4], _hashCodes[4]),
                (_codes[5], _hashCodes[5]),
                (_codes[6], _hashCodes[6]),
                (_codes[7], _hashCodes[7])]);

        _handler = new GenerateMfaArtifactsCommandHandler(_mockUserRepository.Object, _mockTotpProvider.Object, _mockRecoveryCodesProvider.Object);
    }

    [Test]
    public async Task Handle_WhenAllDataValid_ShouldReturnAsExpected()
    {
        // Arrange
        var command = new GenerateMfaArtifactsCommand(UserId.ToString());
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.qr, Is.EqualTo(Qr));
            Assert.That(result.Value.codes, Has.Count.EqualTo(RecoveryCodesCount));
            result.Value.codes.ShouldBeEquivalentTo(_codes);
        }
        _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WhenUserIdInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new GenerateMfaArtifactsCommand(InvalidUserId);
        
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
        var command = new GenerateMfaArtifactsCommand(NonExistentUserId.ToString());
        
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
    public async Task Handle_WhenUserEmailNotConfirmed_ShouldReturnFailure()
    {
        // Arrange
        _user = User.Create(
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
        _mockUserRepository.Setup(repository => repository.GetUserByIdAsync(UserId, _cancellationToken))
            .ReturnsAsync(_user);
        var command = new GenerateMfaArtifactsCommand(UserId.ToString());
        
        // Act
        var result = await _handler.Handle(command, _cancellationToken);
        
        using (Assert.EnterMultipleScope())
        {

            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(Codes.UserEmailNotConfirmedYet));
            _mockUserRepository.Verify(repository => repository.UpdateUserAsync(_user, _cancellationToken), Times.Never);
        }
    }
}