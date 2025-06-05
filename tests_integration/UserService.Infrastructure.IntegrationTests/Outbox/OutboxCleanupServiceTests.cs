using Infrastructure.Options.Outbox;
using Infrastructure.Outbox;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace UserService.Infrastructure.Tests.Outbox;

public class Tests
{
    private NpgsqlDataSource _dataSource;
    private OutboxRepository _outboxRepository;
    
    private Mock<ILogger<OutboxCleanupService>> _outboxLogger;
    private Mock<ILogger<OutboxRepository>> _outboxRepositoryLogger;

    private Mock<IServiceScopeFactory> _serviceScopeFactory;
    private Mock<IServiceScope> _serviceScope;
    private Mock<IServiceProvider> _serviceProvider;
        
    private PostgreSqlContainer _postgres;
    private string _connectionString;
    
    [SetUp]
    public async Task Setup()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:alpine")
            .Build();

        await _postgres.StartAsync();
        _connectionString = _postgres.GetConnectionString();
        
        _dataSource = NpgsqlDataSource.Create(_connectionString);
        
        _outboxLogger = new Mock<ILogger<OutboxCleanupService>>();
        _outboxRepositoryLogger = new Mock<ILogger<OutboxRepository>>();
        
        _outboxRepository = new OutboxRepository(_dataSource, _outboxRepositoryLogger.Object);
        
        _serviceProvider = new Mock<IServiceProvider>();
        _serviceProvider.Setup(sp => sp.GetService(typeof(IOutboxRepository))).Returns(_outboxRepository);
        
        _serviceScope = new Mock<IServiceScope>();
        _serviceScope.Setup(s => s.ServiceProvider).Returns(_serviceProvider.Object);
        
        _serviceScopeFactory = new Mock<IServiceScopeFactory>();
        _serviceScopeFactory.Setup(factory => factory.CreateScope()).Returns(_serviceScope.Object);
        
        await _postgres.ExecScriptAsync(
            """
            CREATE TABLE outbox_messages (
                id UUID PRIMARY KEY,
                type VARCHAR(255) NOT NULL,
                topic VARCHAR(255) NOT NULL,
                content JSONB NOT NULL,
                occurred_on_utc TIMESTAMP WITH TIME ZONE NOT NULL,
                processed_on_utc TIMESTAMP WITH TIME ZONE NULL,
                error TEXT NULL,
                error_count int not null default 0);
            """
            );
    }

    [Test]
    public void OutboxCleanupService_WhenOutboxMessagesIsEmpty_ShouldWorkAndLog()
    {
        // Arrange
        var options = Options.Create(new OutboxOptions { RetentionDays = 30, CleanupPauseHours = 24 });
        var service = new OutboxCleanupService(_serviceScopeFactory.Object, _outboxLogger.Object, options);

        // Act
        service.StartAsync(CancellationToken.None);

        // Assert
        Assert.ThatAsync(() => _outboxRepository.GetPendingAsync(10, CancellationToken.None), Has.Count.EqualTo(0));
        _outboxLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Outbox cleanup service started.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _postgres.DisposeAsync();
        await _dataSource.DisposeAsync();
    }
}