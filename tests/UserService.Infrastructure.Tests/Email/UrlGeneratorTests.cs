using System.Web;
using Domain.Users;
using Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UserService.Infrastructure.Tests.Email;

[TestFixture]
public class UrlGeneratorTests
{
    private const string Token = "sampleToken";
    private readonly Guid _userIdGuid = Guid.CreateVersion7();
    private UserId _userId;
    private const string BaseUrl = "https://example.com";
    private const string DefaultBaseUrl = "https://localhost:5001";
    
    private Mock<IConfiguration> _mockConfiguration;
    private UrlGenerator _urlGenerator;

    [SetUp]
    public void SetUp()
    {
        _userId = new UserId(_userIdGuid);
        
        _mockConfiguration = new Mock<IConfiguration>();
        _urlGenerator = new UrlGenerator(_mockConfiguration.Object);
    }

    [Test]
    public void GenerateEmailConfirmationLink_WithConfiguredBaseUrl_ReturnsCorrectUrl()
    {
        // Arrange
        _mockConfiguration.Setup(config => config["App:BaseUrl"]).Returns(BaseUrl);

        // Act
        var result = _urlGenerator.GenerateEmailConfirmationLink(_userId, Token);

        // Assert
        var expectedUrl = $"{BaseUrl}/Account/ConfirmEmail?userId={_userId.Value}&token={HttpUtility.UrlEncode(Token)}";
        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void GenerateEmailConfirmationLink_WithoutConfiguredBaseUrl_UsesDefaultBaseUrl()
    {
        // Arrange
        _mockConfiguration.Setup(config => config["App:BaseUrl"]).Returns((string)null);

        // Act
        var result = _urlGenerator.GenerateEmailConfirmationLink(_userId, Token);

        // Assert
        var expectedUrl = $"{DefaultBaseUrl}/Account/ConfirmEmail?userId={_userId.Value}&token={HttpUtility.UrlEncode(Token)}";
        Assert.That(result, Is.EqualTo(expectedUrl));
    }
    
    [TestCase("", "emptyToken")]
    [TestCase(null, "nullToken")]
    [TestCase("https://custom.com/", "customBaseWithSlash")]
    public void GenerateEmailConfirmationLink_WithEdgeCaseInputs_HandlesThemCorrectly(string? baseUrl, string testToken)
    {
        // Arrange
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _mockConfiguration.Setup(config => config["App:BaseUrl"]).Returns(baseUrl);
        }

        // Act
        var result = _urlGenerator.GenerateEmailConfirmationLink(_userId, testToken);

        // Assert
        var expectedBaseUrl = string.IsNullOrEmpty(baseUrl) ? DefaultBaseUrl : baseUrl;
        expectedBaseUrl = expectedBaseUrl.TrimEnd('/');
        
        var expectedUrl = $"{expectedBaseUrl}/Account/ConfirmEmail?userId={_userId.Value}&token={HttpUtility.UrlEncode(testToken)}";
        Assert.That(result, Is.EqualTo(expectedUrl));
    }
}