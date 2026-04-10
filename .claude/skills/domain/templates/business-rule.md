# Business Rule — pattern reference

## When to read
You are adding a new invariant to a domain method. Each rule lives in its own file under `Domain/{Aggregate}/Rules/`.

## File location
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/{Aggregate}/Rules/Cannot{X}Rule.cs`

## Rules
- One file per rule.
- Exception class and rule class **co-located** in the same file — exception first, rule second.
- Exception inherits `BaseBusinessRuleException` and **must** declare all 4 ctor overloads: `()`, `(string message)`, `(string message, Exception innerException)`, `(IBusinessRule rule)`.
- Rule implements `IBusinessRule`, uses primary constructor, stores args in `private readonly` fields.
- `IsBroken()` returns a bool against the field(s); `ThrowException()` throws the paired exception with `this`; `Message` is human-readable.

## Template
```csharp
using HC.Core.Domain;

namespace HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Rules;

public class {Action}{X}Exception : BaseBusinessRuleException
{
    public {Action}{X}Exception() { }
    public {Action}{X}Exception(string message) : base(message) { }
    public {Action}{X}Exception(string message, System.Exception innerException) : base(message, innerException) { }
    public {Action}{X}Exception(IBusinessRule rule) : base(rule) { }
}

public class Cannot{X}Rule(
    {StateType} actualState
) : IBusinessRule
{
    private readonly {StateType} _actualState = actualState;
    public bool IsBroken() => /* condition */;
    public void ThrowException() => throw new {Action}{X}Exception(this);
    public string Message => "...";
}
```

## Reference
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/Rules/CannotAcceptOrderItemMoreThanOnceRule.cs`
- `src/HC.Core/Domain/IBusinessRule.cs`
- `src/HC.Core/Domain/BaseBusinessRuleException.cs`
