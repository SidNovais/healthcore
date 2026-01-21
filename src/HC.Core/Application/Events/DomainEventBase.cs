using System;
using HC.Core.Domain;

namespace HC.Core.Application.Events;

public class DomainEventBase : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public DateTime OcurredAt { get; } = SystemClock.Now;
}
