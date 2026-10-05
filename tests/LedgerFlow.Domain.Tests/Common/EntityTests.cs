using LedgerFlow.Domain.Common;

namespace LedgerFlow.Domain.Tests.Common;

public class EntityTests
{
    private sealed class FakeEntity(Guid id) : Entity<Guid>(id)
    {
    }

    private sealed class AnotherFakeEntity(Guid id) : Entity<Guid>(id)
    {
    }

    // Verifies that entities of the same type and same Id are considered equal
    [Fact]
    public void Equals_WhenSameTypeAndSameId_ShouldReturnTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new FakeEntity(id);
        var entity2 = new FakeEntity(id);

        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1.Equals((object)entity2));
        Assert.True(entity1 == entity2);
        Assert.False(entity1 != entity2);
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    // Verifies that different entity types with the same Id are not equal
    [Fact]
    public void Equals_WhenDifferentTypeAndSameId_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var entity1 = new FakeEntity(id);
        var entity2 = new AnotherFakeEntity(id);

        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1.Equals((object)entity2));
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }

    // Verifies that entities of the same type with different Ids are not equal
    [Fact]
    public void Equals_WhenSameTypeAndDifferentId_ShouldReturnFalse()
    {
        var entity1 = new FakeEntity(Guid.NewGuid());
        var entity2 = new FakeEntity(Guid.NewGuid());

        Assert.False(entity1.Equals(entity2));
        Assert.False(entity1.Equals((object)entity2));
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }

    // Verifies that comparing an entity instance against null returns false
    [Fact]
    public void Equals_WhenComparedWithNull_ShouldReturnFalse()
    {
        var entity = new FakeEntity(Guid.NewGuid());

        Assert.False(entity.Equals(null));
        Assert.False(entity == null);
        Assert.False(null == entity);
        Assert.True(entity != null);
        Assert.True(null != entity);
    }
}
