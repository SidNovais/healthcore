# Aggregate root — pattern reference

## When to read
You are adding a public command method to an aggregate root (or creating a new aggregate). Aggregates inherit `AggregateRoot` and use the Apply/When event-sourcing pattern.

## File location
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/{Aggregate}.cs`

## Rules
- Inherits `AggregateRoot` (HC.Core.Domain.EventSourcing).
- Private parameterless constructor (for event sourcing).
- Static `Create(...)` factory — builds the aggregate, raises `{Aggregate}CreatedDomainEvent`, calls `Apply(...)` then `AddDomainEvent(...)`.
- Each command method:
  1. Constructs a domain event with **primitives only** (unwrap ValueObjects via `.Value` / `.Prop`).
  2. Calls `Apply(domainEvent)`.
  3. Calls `AddDomainEvent(domainEvent)`.
- `protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);`
- One private `When({EventType} domainEvent)` handler per event — either updates aggregate state directly, or delegates to a child entity (`_items.Single(...).{Action}(domainEvent)`).

## Template
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Events;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

public class {Aggregate} : AggregateRoot
{
    private {ValueObject} _someField = null!;
    private IList<{Entity}> _items = [];
    private DateTime _createdAt;

    private {Aggregate}() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static {Aggregate} Create(
        Guid id,
        {ValueObject} someField,
        DateTime createdAt)
    {
        {Aggregate} agg = new();
        {Aggregate}CreatedDomainEvent ev = new(
            id,
            someField.Prop1,  // unwrap ValueObject
            someField.Prop2,
            createdAt);
        agg.Apply(ev);
        agg.AddDomainEvent(ev);
        return agg;
    }

    public void {Action}({Entity}Id entityId, /* params */)
    {
        {Aggregate}{Action}DomainEvent ev = new(entityId.Value, /* params */);
        Apply(ev);
        AddDomainEvent(ev);
    }

    private void When({Aggregate}CreatedDomainEvent domainEvent)
    {
        Id = domainEvent.{Aggregate}Id;
        _someField = {ValueObject}.Of(domainEvent.Prop1, domainEvent.Prop2);
        _createdAt = domainEvent.CreatedAt;
    }

    // Delegating to a child entity:
    private void When({Aggregate}{Action}DomainEvent domainEvent)
        => _items.Single(i => i.{Entity}Id.Value == domainEvent.{Entity}Id).{Action}(domainEvent);
}
```

## Integration notes
- **ValueObject unwrap:** if a command method reads a ValueObject field (e.g. `_status`), call `.Value` or expand its props — events carry primitives only.
- **Reconstruct in `When`:** `_field = {ValueObject}.Of(domainEvent.X, ...)` — never `new`.

## Reference
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/Order.cs` — full aggregate: `Create()`, command methods (`AcceptExam`, `CancelExam`, …), Apply dispatch, delegating `When` handlers
- `src/HC.Core/Domain/EventSourcing/AggregateRoot.cs` — base: `AddDomainEvent()`, abstract `Apply()`, `Load()` for replay
