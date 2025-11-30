using System;

namespace HC.Core.Domain;

public class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTime OcurredAt { get; } = SystemClock.Now;
}
