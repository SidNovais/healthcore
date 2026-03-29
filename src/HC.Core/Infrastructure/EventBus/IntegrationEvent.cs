using System;
using MediatR;

namespace HC.Core.Infrastructure.EventBus;

public abstract class IntegrationEvent(
    Guid id,
    DateTime occurredAt
) : INotification
{
    public Guid Id { get; } = id;

    public DateTime OccurredAt { get; } = occurredAt;
}
