using Application.Abstractions.Authentication;
using Infrastructure.Authentication;
using Moq;

namespace UserService.Infrastructure.Tests.Authentication;

[TestFixture]
public class RecoveryCodesProviderTests
{
    private const string Code = "test-code";
    private const string Hash = "hashed_test-code";
    
    private Mock<IPasswordHasher> _hasherMock;
    private RecoveryCodesProvider _recoveryCodesProvider;

    [SetUp]
    public void SetUp()
    {
        _hasherMock = new Mock<IPasswordHasher>();
        _recoveryCodesProvider = new RecoveryCodesProvider(_hasherMock.Object);
    }

    [Test]
    public void GenerateRecoveryCodes_WithValidCount_ShouldReturnCorrectNumberOfCodes()
    {
        // Arrange
        const int count = 8;
        _hasherMock.Setup(h => h.GetHash(It.IsAny<string>())).Returns((string s) => "hashed_" + s);

        // Act
        var codes = _recoveryCodesProvider.GenerateRecoveryCodes();

        // Assert
        Assert.That(codes, Has.Count.EqualTo(count));
        foreach (var code in codes)
        {
            Assert.That(code.hashCode, Does.StartWith("hashed_"));
        }
        
        var uniqueCodes = codes.Select(c => c.code).Distinct();
        Assert.That(uniqueCodes.Count(), Is.EqualTo(count), "All generated codes should be unique");
        
        _hasherMock.Verify(h => h.GetHash(It.IsAny<string>()), Times.Exactly(count));
    }
    
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void GenerateRecoveryCodes_WithDifferentCounts_ShouldReturnCorrectNumberOfCodes(int count)
    {
        // Arrange
        _hasherMock.Setup(h => h.GetHash(It.IsAny<string>())).Returns((string s) => "hashed_" + s);

        // Act
        var codes = _recoveryCodesProvider.GenerateRecoveryCodes(count);

        // Assert
        Assert.That(codes, Has.Count.EqualTo(count));
    }
    
    [Test]
    public void GenerateRecoveryCodes_WithNegativeCount_ShouldThrowArgumentException()
    {
        // Arrange
        const int negativeCount = -1;
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _recoveryCodesProvider.GenerateRecoveryCodes(negativeCount));
    }

    [Test]
    public void VerifyRecoveryCode_WithValidCode_ShouldReturnTrue()
    {
        // Arrange
        _hasherMock.Setup(h => h.CheckPassword(Hash, Code)).Returns(true);

        // Act
        var result = _recoveryCodesProvider.VerifyRecoveryCode(Code, Hash);

        // Assert
        Assert.That(result, Is.True);
        _hasherMock.Verify(h => h.CheckPassword(Hash, Code), Times.Once);
    }

    [Test]
    public void VerifyRecoveryCode_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        _hasherMock.Setup(h => h.CheckPassword(Hash, Code)).Returns(false);

        // Act
        var result = _recoveryCodesProvider.VerifyRecoveryCode(Code, Hash);

        // Assert
        Assert.That(result, Is.False);
        _hasherMock.Verify(h => h.CheckPassword(Hash, Code), Times.Once);
    }
}