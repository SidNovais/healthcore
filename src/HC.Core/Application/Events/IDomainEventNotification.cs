using System;
using MediatR;

namespace HC.Core.Application;

public interface IDomainEventNotification<out T> : IDomainEventNotification
{
    T EventNotification { get; }
}

public interface IDomainEventNotification : INotification
{
    public Guid Id { get; }
}
