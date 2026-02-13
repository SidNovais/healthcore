using System;
using HC.Core.Domain;

namespace HC.Core.Application.Events;

public class DomainNotificationBase<T>(T domainEvent, Guid id) : IDomainEventNotification<T>
    where T : IDomainEvent
{
    public T DomainEvent { get; } = domainEvent;
    public Guid Id { get; } = id;

}
