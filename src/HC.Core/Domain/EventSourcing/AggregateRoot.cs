using System;
using System.Collections.Generic;

namespace HC.Core.Domain.EventSourcing;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _events;

    public Guid Id { get; protected set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _events.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _events.Add(@event);

    protected AggregateRoot()
    {
        _events = [];
        Version = -1;
    }

    public void Load(IEnumerable<IDomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history, "history cannot be null");
        foreach (IDomainEvent e in history)
        {
            Apply(e);
            Version++;
        }
    }

    protected abstract void Apply(IDomainEvent domainEvent);

    protected static void CheckRule(IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule, "rule cannot be null");
        if (rule.IsBroken()) throw new BaseBusinessRuleException(rule);
    }
}
