using Domain.Roles;
using Infrastructure.Caching.Interfaces;
using Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace UserService.Infrastructure.Tests.Healthchecks;

[TestFixture]
public class CacheHealthCheckTests
{
    private Mock<ICacheService> _mockCacheService;
    private CacheHealthCheck _cacheHealthCheck;

    [SetUp]
    public void SetUp()
    {
        _mockCacheService = new Mock<ICacheService>();  
        _cacheHealthCheck = new CacheHealthCheck(_mockCacheService.Object);
    }

    [Test]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenCacheOperationsSucceed()
    {
        // Arrange
        _mockCacheService.Setup(x => x.GetEntity<Role>(It.IsAny<string>())).Returns(Role.CreateRole(Guid.CreateVersion7(), "name"));
        _mockCacheService.Setup(x => x.Remove(It.IsAny<string>()));
        _mockCacheService.Setup(x => 
            x.Create(
                It.IsAny<string>(), 
                It.IsAny<Role>(), 
                null, 
                null));
            
        // Act
        var result = await _cacheHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(result.Description, Is.EqualTo("Cache instance is healthy."));
        }
    }

    [Test]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenCacheOperationsThrowException()
    {
        // Arrange
        _mockCacheService.Setup(x => 
            x.Create(
                It.IsAny<string>(), 
                It.IsAny<Role>(), 
                null, 
                null)).Throws(new InvalidOperationException("Cache error."));

        // Act
        var result = await _cacheHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(result.Description, Is.EqualTo("Cache instance is unhealthy."));
            Assert.That(result.Exception, Is.Not.Null);
        }
    }
}