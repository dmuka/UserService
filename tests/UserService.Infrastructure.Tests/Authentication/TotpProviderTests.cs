using Infrastructure.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using OtpNet;

namespace UserService.Infrastructure.Tests.Authentication;

[TestFixture]
public class TotpProviderTests
{
    private const string SecretKey = "JBSWY3DPEHPK3PXP";
    private const string UserEmail = "user@example.com";
    private const string Issuer = "TestIssuer";
    
    
    private TestLogger<TotpProvider> _loggerMock;
    private TotpProvider _totpProvider;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new TestLogger<TotpProvider>();
        _totpProvider = new TotpProvider(_loggerMock);
    }

    [Test]
    public void GenerateSecretKey_ShouldReturnBase32EncodedKey()
    {
        // Act
        var secretKey = _totpProvider.GenerateSecretKey();

        // Assert
        Assert.That(secretKey, Is.Not.Null.And.Not.Empty);
        Assert.That(secretKey, Has.Length.EqualTo(32));
    }

    [Test]
    public void GetQr_ShouldReturnBase64EncodedQrCode()
    {
        // Arrange & Act
        var qrCode = _totpProvider.GetQr(SecretKey, UserEmail, Issuer);

        // Assert
        Assert.That(qrCode, Is.Not.Null.And.Not.Empty);
        Assert.That(qrCode, Does.StartWith("iVBORw0KGgo")); // PNG file signature in Base64
    }

    [Test]
    public void ValidateTotp_ShouldReturnTrueForValidCode()
    {
        // Arrange
        var totp = new Totp(Base32Encoding.ToBytes(SecretKey));
        var validCode = totp.ComputeTotp();

        // Act
        var result = _totpProvider.ValidateTotp(SecretKey, int.Parse(validCode));

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(_loggerMock.LoggedMessages, Has.Count.EqualTo(1));
            Assert.That(_loggerMock.LoggedMessages[0], Does.StartWith("TOTP validation used time window: "));
        }
    }

    [Test]
    public void ValidateTotp_ShouldReturnFalseForInvalidCode()
    {
        // Arrange
        const int invalidCode = 123456;

        // Act
        var result = _totpProvider.ValidateTotp(SecretKey, invalidCode);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.False);
            Assert.That(_loggerMock.LoggedMessages, Has.Count.EqualTo(1));
            Assert.That(_loggerMock.LoggedMessages[0], Does.StartWith("TOTP validation used time window: "));
        }
    }
    
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            LoggedMessages.Add(formatter(state, exception));
        }

        public void LogInformation(string? message, params object?[] args)
        {
            LoggedMessages.Add(string.Format(message, args));
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }
}