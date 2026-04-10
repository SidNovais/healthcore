# Entity (with strongly-typed ID) — pattern reference

## When to read
You are creating a new entity (or aggregate child) that has identity surviving state transitions. If it has no identity, use `value-object.md` instead.

## File locations
- `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/{Entity}Id.cs`
- `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/{Entity}.cs`

## Naming rule (cross-module IDs)
The strongly-typed ID **must use a name from the local module's vocabulary**, not the upstream module's name. The same `Guid` value can flow across modules under different local names — e.g., `TestOrders.Order.OrderId` and `SampleCollection.SampleRequest.RequestId` carry the same value but each module names it for its own concept. Each bounded context owns the vocabulary for its own data.

## Rules
- Entity inherits `Entity` (HC.Core.Domain).
- Strongly-typed ID inherits `Id` (HC.Core.Domain) — the base ctor throws on `Guid.Empty`.
- ID class is a one-line primary-constructor declaration.
- Entity exposes the ID as `internal {Entity}Id {Entity}Id { get; private set; }`.
- Domain events still carry the raw `Guid` (primitives only). The entity wraps it into the local ID type inside the `When()` handler.
- State-transition methods use `CheckRule(...)` then `Apply(domainEvent)`; field updates happen in `When(...)` handlers.

## Template

### Strongly-typed ID
```csharp
using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

public class {Entity}Id(Guid value) : Id(value) { }
```

### Entity
```csharp
using HC.Core.Domain;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Events;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Rules;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

public class {Entity} : Entity
{
    internal {Entity}Id {Entity}Id { get; private set; }
    internal {EntityStatus} _status;
    // ... other internal fields

    private {Entity}() { }

    internal static {Entity} {Action}({Entity}{Action}DomainEvent domainEvent)
    {
        {Entity} entity = new();
        entity.Apply(domainEvent);
        return entity;
    }

    internal void {NextAction}({Entity}{NextAction}DomainEvent domainEvent)
    {
        CheckRule(new Cannot{X}Rule(_status));
        // ... additional rules
        Apply(domainEvent);
    }

    private void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    private void When({Entity}{Action}DomainEvent domainEvent)
    {
        {Entity}Id = new(domainEvent.{Entity}Id);
        _status = {EntityStatus}.{Initial};
        // ...
    }

    private void When({Entity}{NextAction}DomainEvent domainEvent)
    {
        _status = {EntityStatus}.{Next};
        // ...
    }
}
```

## Reference
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/OrderItemId.cs` — strongly-typed ID one-liner
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/OrderItem.cs` — entity with `CheckRule` + `Apply` + `When` flow
- `src/HC.Core/Domain/Id.cs` — base class
- `src/HC.Core/Domain/Entity.cs` — `CheckRule()`, `Events`, `AddEvent()`, `ClearEvents()`
