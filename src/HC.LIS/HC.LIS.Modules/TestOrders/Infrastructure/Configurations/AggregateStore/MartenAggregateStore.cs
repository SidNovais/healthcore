using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.AggregateStore;

public class MartenAggregateStore(
  IDocumentSession documentSession
) : IAggregateStore
{
    private readonly IDocumentSession _documentSession = documentSession;
    private readonly List<IDomainEvent> _events = [];
    public void AppendChanges<T>(T aggregate) where T : AggregateRoot
    {
        IReadOnlyCollection<IDomainEvent> events = aggregate.GetDomainEvents();
        _documentSession.Events.Append(GetStreamId(aggregate), events);
        _events.AddRange(events);
    }

    public void Start<T>(T aggregate) where T : AggregateRoot
    {
        IReadOnlyCollection<IDomainEvent> events = aggregate.GetDomainEvents();
        _documentSession.Events.StartStream<T>(GetStreamId(aggregate), aggregate.GetDomainEvents());
        _events.AddRange(events);
    }

    public async Task<T?> Load<T>(AggregateId<T> aggregateId) where T : AggregateRoot
    {
        string streamId = GetStreamId(aggregateId);
        IReadOnlyList<IEvent> events = await _documentSession.Events.FetchStreamAsync(streamId).ConfigureAwait(false);
        IList<IDomainEvent> domainEvents = [];
        foreach (IEvent @event in events)
        {
            Type type = DomainEventTypeMappings.Dictionary[@event.EventType.Name];
            string json = JsonConvert.SerializeObject(@event.Data);
            var domainEvent = JsonConvert.DeserializeObject(json, type) as IDomainEvent;
            domainEvents.Add(domainEvent!);
        }
        if (!domainEvents.Any()) return null;
        T? aggregate = (T?)Activator.CreateInstance(typeof(T), true);
        if (aggregate is null) return null;
        aggregate.Load(domainEvents);
        return aggregate;
    }

    public IList<IDomainEvent> GetChanges() => _events;

    public void ClearChanges() => _events.Clear();

    private static string GetStreamId<T>(T aggregate)
      where T : AggregateRoot
      => $"{aggregate.GetType().Name}-{aggregate.Id:N}";

    private static string GetStreamId<T>(AggregateId<T> aggregateId)
      where T : AggregateRoot
      => $"{typeof(T).Name}-{aggregateId.Value:N}";
}
