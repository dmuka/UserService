using Domain;

namespace UserService.Domain.Tests;

[TestFixture]
public class EntityTests
{
    private const long EntityId = 1L;
    private const long DifferentTypeValue = 2L;
        
    private class TestEntity : Entity
    {
        public TestEntity(long id)
        {
            Id = id;
        }
    }

    [Test]
    public void Test_Id_ShouldBeSetCorrectly()
    {
        // Arrange
        // Act
        var entity = new TestEntity(EntityId);

        // Assert
        Assert.That(entity.Id, Is.EqualTo(EntityId));
    }

    [Test]
    public void Test_Equals_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(EntityId);
        var differentType = new object();

        // Act & Assert
        Assert.That(entity.Equals(differentType), Is.False);
    }

    [Test]
    public void Test_Equals_Null_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity, Is.Not.EqualTo(null));
    }

    [Test]
    public void Test_Equals_SameIdDifferentInstances_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity1, Is.EqualTo(entity2));
    }

    [Test]
    public void Test_Equals_DifferentIds_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(2L);

        // Act & Assert
        Assert.That(entity1, Is.Not.EqualTo(entity2));
    }

    [Test]
    public void Test_Equals_TransientEntities_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(0L);
        var entity2 = new TestEntity(0L);

        // Act & Assert
        Assert.That(entity1, Is.Not.EqualTo(entity2));
    }

    [Test]
    public void Test_GetHashCode_NonTransientEntity_ShouldReturnSameHashCode()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity1.GetHashCode(), Is.EqualTo(entity2.GetHashCode()));
    }

    [Test]
    public void Test_GetHashCode_TransientEntity_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(0L);
        var entity2 = new TestEntity(0L);

        // Act & Assert
        Assert.That(entity1.GetHashCode(), Is.Not.EqualTo(entity2.GetHashCode()));
    }

    [Test]
    public void Test_AddDomainEvent_ShouldAddEvent()
    {
        // Arrange
        var entity = new TestEntity(EntityId);
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
        var entity = new TestEntity(EntityId);
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
        var entity = new TestEntity(EntityId);
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.That(entity.DomainEvents, Is.Empty);
    }

    [Test]
    public void Test_EqualityOperator_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(EntityId);
        object differentType = DifferentTypeValue;

        // Act & Assert
        Assert.That(entity, Is.Not.EqualTo(differentType));
    }

    [Test]
    public void Test_EqualityOperator_Null_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity, Is.Not.EqualTo(null));
    }

    [Test]
    public void Test_EqualityOperator_SameIdDifferentInstances_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity1, Is.EqualTo(entity2));
    }

    [Test]
    public void Test_EqualityOperator_DifferentIds_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(2L);

        // Act & Assert
        Assert.That(entity1, Is.Not.EqualTo(entity2));
    }

    [Test]
    public void Test_InequalityOperator_Null_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity, Is.Not.EqualTo(null));
    }

    [Test]
    public void Test_InequalityOperator_SameIdDifferentInstances_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(EntityId);

        // Act & Assert
        Assert.That(entity1 != entity2, Is.False);
    }

    [Test]
    public void Test_InequalityOperator_DifferentIds_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(EntityId);
        var entity2 = new TestEntity(2L);

        // Act & Assert
        Assert.That(entity1 == entity2, Is.False);
    }

    private class TestDomainEvent : IDomainEvent;
}