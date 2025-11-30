using System;
using HC.Core.Domain;

namespace HC.Core.Application;

public class DomainEventBase<T>(Guid id, T eventNotification) : IDomainEventNotification<T>
    where T : IDomainEvent
{
    public Guid Id { get; } = id;
    public T EventNotification { get; } = eventNotification;
}
