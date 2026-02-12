using System;
using HC.Core.Domain;

namespace HC.Core.Application.Events;

public class DomainNotificationBase<T>(T domainEvent, Guid id) : IDomainEventNotification<T>
    where T : IDomainEvent
{
    public Guid Id { get; } = id;

    public DateTime OcurredAt { get; } = SystemClock.Now;

    public T EventNotification => domainEvent;
}
