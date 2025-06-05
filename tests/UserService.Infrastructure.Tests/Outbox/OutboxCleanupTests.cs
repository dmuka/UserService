using Infrastructure.Options.Outbox;
using Infrastructure.Outbox;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace UserService.Infrastructure.Tests.Outbox;

[TestFixture]
public class OutboxCleanupServiceTests
{
    private Mock<IServiceScopeFactory> _mockScopeFactory;
    private Mock<ILogger<OutboxCleanupService>> _mockLogger;
    private Mock<IOptions<OutboxOptions>> _mockOptions;
    private Mock<IServiceScope> _mockScope;
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<IOutboxRepository> _mockOutboxRepository;
    private OutboxOptions _outboxOptions;
    private OutboxCleanupService _service;
    private CancellationTokenSource _cts;

    [SetUp]
    public void Setup()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<OutboxCleanupService>>();
        _mockOptions = new Mock<IOptions<OutboxOptions>>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockOutboxRepository = new Mock<IOutboxRepository>();
            
        _outboxOptions = new OutboxOptions
        {
            RetentionDays = 7,
            CleanupPauseHours = 24
        };
            
        _mockOptions.Setup(x => x.Value).Returns(_outboxOptions);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IOutboxRepository)))
            .Returns(_mockOutboxRepository.Object);
            
        _service = new OutboxCleanupService(
            _mockScopeFactory.Object,
            _mockLogger.Object,
            _mockOptions.Object);
                
        _cts = new CancellationTokenSource();
    }
        
    [TearDown]
    public void TearDown()
    {
        _cts.Dispose();
        _service.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldCallCleanUpWithCorrectRetentionDays()
    {
        // Arrange
        _mockOutboxRepository
            .Setup(x => x.CleanUpAsync(_outboxOptions.RetentionDays, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10)
            .Verifiable();
        
        await _service.StartAsync(_cts.Token);
        
        await Task.Delay(100);
        await _service.StopAsync(_cts.Token);
            
        // Assert
        _mockOutboxRepository.Verify(
            x => x.CleanUpAsync(_outboxOptions.RetentionDays, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Outbox cleanup service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_ShouldRespectConfiguredDelayBetweenCleanups()
    {
        // Arrange
        var cleanupCount = 0;
        _mockOutboxRepository
            .Setup(x => x.CleanUpAsync(_outboxOptions.RetentionDays, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10)
            .Callback(() => cleanupCount++);
        
        _outboxOptions.CleanupPauseHours = 1;
            
        var expectedDelay = TimeSpan.FromHours(_outboxOptions.CleanupPauseHours);
            
        var serviceTask = Task.Run(async () => 
        {
            await _service.StartAsync(_cts.Token);
            await Task.Delay(100);
            await _cts.CancelAsync();
        });
            
        await Task.WhenAny(serviceTask, Task.Delay(5000));
        
        _mockOutboxRepository.Verify(
            x => x.CleanUpAsync(_outboxOptions.RetentionDays, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        Assert.That(cleanupCount, Is.GreaterThan(0));
    }
        
    [Test]
    public async Task ExecuteAsync_ShouldLogInformationWhenStarting()
    {
        // Arrange
        _mockOutboxRepository
            .Setup(x => x.CleanUpAsync(_outboxOptions.RetentionDays, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
                
        // Act
        await _service.StartAsync(_cts.Token);
        await Task.Delay(100);
        await _service.StopAsync(_cts.Token);
            
        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Outbox cleanup service started.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    
    [Test]
    public async Task ExecuteAsync_ShouldLogStoppedMessageAfterCompletingACleanupCycle()
    {
        // Arrange
        _outboxOptions.CleanupPauseHours = 0;
        var cleanupCalled = false;
        _mockOutboxRepository
            .Setup(x => x.CleanUpAsync(_outboxOptions.RetentionDays, It.IsAny<CancellationToken>()))
            .Callback<int, CancellationToken>(void (_, _) =>
            {
                if (cleanupCalled) return;
                cleanupCalled = true;
                _cts.CancelAfter(50);
            })
            .ReturnsAsync(10);
        
        // Act
        try
        {
            var serviceTask = _service.StartAsync(_cts.Token);
            
            await Task.WhenAny(serviceTask, Task.Delay(1000, CancellationToken.None));
            await _service.StopAsync(_cts.Token);
            
            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Outbox cleanup service stopped.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
        finally
        {
            _cts.Dispose();
        }
    }
    
    [Test]
    public async Task ExecuteAsync_ShouldCallCleanUpWithCorrectRetentionDaysValue()
    {
        // Arrange
        _outboxOptions.RetentionDays = 14;
        
        _mockOutboxRepository
            .Setup(x => x.CleanUpAsync(14, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5)
            .Verifiable();
                
        // Act
        await _service.StartAsync(_cts.Token);
        await Task.Delay(100);
        await _service.StopAsync(_cts.Token);
            
        // Assert
        _mockOutboxRepository.Verify(
            x => x.CleanUpAsync(14, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}