using Core;

namespace UserService.Core.Tests;

[TestFixture]
public class EntityTests
{
    private class TestEntity : Entity
    {
    }

    [Test]
    public void Test_Equals_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity();
        var differentType = new object();

        // Act & Assert
        Assert.That(entity.Equals(differentType), Is.False);
    }

    [Test]
    public void Test_Equals_Null_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        Assert.That(entity, Is.Not.EqualTo(null));
    }

    [Test]
    public void Test_AddDomainEvent_ShouldAddEvent()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        Assert.That(entity.DomainEvents, Does.Contain(domainEvent));
    }

    [Test]
    public void Test_RemoveDomainEvent_ShouldRemoveEvent()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        Assert.That(entity.DomainEvents, Does.Not.Contain(domainEvent));
    }

    [Test]
    public void Test_ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.That(entity.DomainEvents, Is.Empty);
    }

    private class TestDomainEvent : IDomainEvent;
}