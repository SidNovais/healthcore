using System;
using System.Collections.Generic;

namespace HC.Core.Domain;

public abstract class Entity
{
    private IList<IDomainEvent> _events = [];

    public IReadOnlyCollection<IDomainEvent> Events => _events.AsReadOnly();

    public void ClearEvents() => _events = [];

    protected void AddEvent(IDomainEvent @event) => _events.Add(@event);

    protected static void CheckRule(IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule, "IBusinessRule argument cannot be null");
        if (rule.IsBroken()) rule.ThrowException();
    }

}
