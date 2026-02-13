using System;
using MediatR;

namespace HC.Core.Application.Events;

public interface IDomainEventNotification<out T> : IDomainEventNotification
{
    T DomainEvent { get; }
}

public interface IDomainEventNotification : INotification
{
    public Guid Id { get; }
}
