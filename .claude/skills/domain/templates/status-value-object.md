# Status ValueObject — pattern reference

## When to create

Create `{Aggregate}Status.cs` when the feature introduces a **new** status type or adds
values to an existing one.
**Never** store status as a plain `string` on an aggregate or entity field.

## File location

`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/{Aggregate}Status.cs`

## Template

```csharp
using HC.Core.Domain;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

public class {Aggregate}Status : ValueObject
{
    public string Value { get; }

    public static {Aggregate}Status Pending => new("Pending");
    public static {Aggregate}Status Active  => new("Active");
    // ... one static property per valid state

    private {Aggregate}Status(string value) => Value = value;

    public static {Aggregate}Status Of(string value) => new(value);

    internal bool IsPending => Value == "Pending";
    internal bool IsActive  => Value == "Active";
    // ... mirror every static property with an internal bool
}
```

## Integration rules

| Location | Rule |
|---|---|
| Domain event | Carry `status.Value` (string primitive) — never the ValueObject itself |
| `When()` handler | Reconstruct via `_status = {Aggregate}Status.Of(domainEvent.Status)` |
| Business rule | Accept `{Aggregate}Status` parameter; use `Is{State}` internally |

## Real examples

- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/OrderItemStatus.cs`
- `src/HC.LIS/HC.LIS.Modules/SampleCollection/Domain/Collections/SampleStatus.cs`
