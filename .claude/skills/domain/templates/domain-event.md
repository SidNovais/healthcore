# Domain Event — pattern reference

## When to read
You are creating a new `*DomainEvent` class under `Domain/{Aggregate}/Events/`.

## File location
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/Events/{Aggregate}{Action}DomainEvent.cs`

## Rules
- Inherits `DomainEvent` from `HC.Core.Domain`.
- Primary-constructor syntax (C# 12+); properties initialized inline from ctor params.
- Carries **primitives only** (`Guid`, `string`, `DateTime`, `bool`, `int`, `decimal`) — never ValueObjects or entities. Unwrap at the call site.
- File-scoped namespace.

## Template
```csharp
using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Events;

public class {Aggregate}{Action}DomainEvent(
    Guid {aggregateId},
    // ... other primitive params
) : DomainEvent
{
    public Guid {AggregateId} { get; } = {aggregateId};
    // ... other properties
}
```

## Reference
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/Events/OrderItemAcceptedDomainEvent.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/Events/OrderItemRequestedDomainEvent.cs` (multi-param, with ValueObject unwrapped to primitives)
