using System.Data;
using Infrastructure.HealthChecks;
using Infrastructure.Options.Db;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;

namespace UserService.Infrastructure.Tests.Healthchecks;

[TestFixture]
public class PostgresHealthCheckTests
{
    private PostgresHealthCheck _postgresHealthCheck;
    private Mock<INpgsqlConnectionFactory> _mockConnectionFactory;
    private Mock<IDbConnection> _mockConnection;
    private Mock<IDbCommand> _mockCommand;
    private Mock<IOptions<PostgresOptions>> _mockOptions; 

    [SetUp]
    public void SetUp()
    {
        _mockOptions = new Mock<IOptions<PostgresOptions>>();
        _mockOptions.Setup(o => o.Value)
            .Returns(new PostgresOptions
            {
                Database = "db", Host = "host", Password = "password", Port = 5432, UserName = "username"
            });
        
        _mockConnectionFactory = new Mock<INpgsqlConnectionFactory>();
        
        _mockCommand = new Mock<IDbCommand>();
        _mockCommand.Setup(command => command.ExecuteScalar()).Returns(1);
        
        _mockConnection = new Mock<IDbConnection>();
        _mockConnection.Setup(connection => connection.Open());
        _mockConnection.Setup(connection => connection.CreateCommand()).Returns(_mockCommand.Object);
        
        _postgresHealthCheck = new PostgresHealthCheck(_mockConnectionFactory.Object, _mockOptions.Object);
    }

    [Test]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenConnectionSucceeds()
    {
        // Arrange
        _mockConnectionFactory.Setup(factory => factory.CreateConnection(_mockOptions.Object.Value.GetConnectionString()))
            .Returns(_mockConnection.Object);

        // Act
        var result = await _postgresHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(result.Description, Is.EqualTo("PostgreSQL instance is healthy."));
        }
    }

    [Test]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenConnectionFails()
    {
        // Arrange
        // Act
        var result = await _postgresHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(result.Description, Is.EqualTo("PostgreSQL instance is unhealthy."));
            Assert.That(result.Exception, Is.Not.Null);
        }
    }
}