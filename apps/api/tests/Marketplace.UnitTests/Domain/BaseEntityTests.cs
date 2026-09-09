using FluentAssertions;
using Marketplace.Domain.Common;
using Xunit;

namespace Marketplace.UnitTests.Domain;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
    }

    [Fact]
    public void An_entity_is_equal_to_itself()
    {
        var entity = new TestEntity();

        entity.Equals(entity).Should().BeTrue();
        (entity == entity).Should().BeTrue();
    }

    [Fact]
    public void Two_freshly_created_entities_have_different_ids_and_are_not_equal()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        entity1.Equals(entity2).Should().BeFalse();
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void An_entity_is_never_equal_to_null()
    {
        var entity = new TestEntity();

        entity.Equals(null).Should().BeFalse();
    }
}
