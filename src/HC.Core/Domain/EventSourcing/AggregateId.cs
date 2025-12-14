using System;

namespace HC.Core.Domain.EventSourcing;

public abstract class AggregateId<T>(Guid value)
    where T : AggregateRoot
{
    public Guid Value { get; } = value;
}
