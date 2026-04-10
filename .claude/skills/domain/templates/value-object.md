# Data-grouping ValueObject — pattern reference

## When to read
You are grouping 2+ related primitive fields that always travel together and describe a single concept (e.g., patient demographics, address, measurement+unit). The cluster has **no identity** (no `Id`) — otherwise use the Entity template instead.

For status types, use `status-value-object.md` instead.

## Why
Reduces field sprawl on the aggregate, clarifies intent, makes the aggregate's `Create()` signature cleaner.

## File location
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/{Name}.cs`

## Rules (non-negotiable)
- Inherits `ValueObject` (HC.Core.Domain).
- All properties are `{ get; }` only — fully immutable.
- **Constructor is `private`** — never expose `new {Name}(...)` to callers.
- Public **static `Of(...)` factory** is the only construction path.

## Template
```csharp
using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

public class {Name} : ValueObject
{
    public {Type1} {Prop1} { get; }
    public {Type2} {Prop2} { get; }
    // ... all related properties

    private {Name}(
        {Type1} prop1,
        {Type2} prop2
        // ...
    )
    {
        {Prop1} = prop1;
        {Prop2} = prop2;
    }

    public static {Name} Of(
        {Type1} prop1,
        {Type2} prop2
        // ...
    ) => new(
        prop1,
        prop2
    );
}
```

## Integration rules

| Location | Rule |
|---|---|
| Aggregate field | `private {Name} _{fieldName} = null!;` |
| `Create()` factory | Accept `{Name}` as a single parameter; unwrap into primitives for the domain event |
| Domain event | Carries **primitives only** — never the ValueObject itself |
| `When()` handler | Reconstruct via `_{fieldName} = {Name}.Of(domainEvent.Prop1, ...)` — never `new` |

## Reference
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/SpecimenRequirement.cs` — canonical shape
- `src/HC.LIS/HC.LIS.Modules/Analyzer/Domain/AnalyzerSamples/PatientInfo.cs` — same pattern, patient demographics
- `src/HC.Core/Domain/ValueObject.cs` — base class (reflection-based equality)
