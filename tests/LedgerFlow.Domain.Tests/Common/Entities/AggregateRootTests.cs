using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Common.Interfaces;

namespace LedgerFlow.Domain.Tests.Common;

public class AggregateRootTests
{
    private sealed record FakeDomainEvent : IDomainEvent;

    private sealed class FakeAggregateRoot(Guid id) : AggregateRoot<Guid>(id)
    {
        public void DoSomething()
        {
            AddDomainEvent(new FakeDomainEvent());
        }
    }

    // Verifies that adding a domain event records it in the internal collection
    [Fact]
    public void AddDomainEvent_ShouldAppendEventToDomainEvents()
    {
        var aggregate = new FakeAggregateRoot(Guid.NewGuid());

        aggregate.DoSomething();

        Assert.Single(aggregate.DomainEvents);
    }

    // Verifies that clearing domain events empties the recorded events list
    [Fact]
    public void ClearDomainEvents_ShouldEmptyDomainEvents()
    {
        var aggregate = new FakeAggregateRoot(Guid.NewGuid());
        aggregate.DoSomething();

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }
}
