using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.Core.Infrastructure.DomainEventsDispatching;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.AggregateStore;

public class AggregateStoreDomainEventsAccessor(
  IAggregateStore aggregateStore
) : IDomainEventsAccessor
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public IReadOnlyCollection<IDomainEvent> GetAllDomainEvents()
        => _aggregateStore.GetChanges().ToList().AsReadOnly();

    public void ClearAllDomainEvents() => _aggregateStore.ClearChanges();
}
