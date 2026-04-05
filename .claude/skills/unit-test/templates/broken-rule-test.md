# Broken-rule test templates

## Style detection

Read the existing `{Aggregate}Tests.cs` before choosing a style:
- Existing tests use `void action() { ... } AssertBrokenRule<Rule>(action);` → use **Style A**
- Existing tests use `AssertBrokenRule<Rule>(() => item.Method(...));` → use **Style B**
- No existing tests → default to **Style A** (matches CLAUDE.md canonical example)

## Naming

Match the existing broken-rule test naming in the file:
- TestOrders pattern: `{Action}ShouldBroke{RuleName}When{Condition}`
- LabAnalysis pattern: `{Action}ThrowsWhen{Condition}` or `{Action}ThrowsOn{Condition}`

---

## Style A — `void action()` delegate (idempotency rule)

```csharp
[Fact]
public void {Action}ShouldBroke{RuleName}When{Condition}()
{
    DateTime actionedAt = SystemClock.Now;
    _sut.{Action}(new {EntityId}({SampleData}.{EntityIdField}), actionedAt);

    void action()
    {
        _sut.{Action}(new {EntityId}({SampleData}.{EntityIdField}), actionedAt);
    }
    AssertBrokenRule<{RuleName}>(action);
}
```

## Style A — `void action()` delegate (state-guard rule, needs factory state)

```csharp
[Fact]
public void {Action}ShouldBroke{RuleName}When{Condition}()
{
    {Aggregate} sut = {Aggregate}Factory.CreateWith{PrerequisiteState}();

    void action()
    {
        sut.{Action}(new {EntityId}({SampleData}.{EntityIdField}), SystemClock.Now);
    }
    AssertBrokenRule<{RuleName}>(action);
}
```

---

## Style B — lambda delegate

```csharp
[Fact]
public void {Action}ThrowsWhen{Condition}()
{
    {Aggregate} item = {Aggregate}Factory.CreateWith{PrerequisiteState}();

    AssertBrokenRule<{RuleName}>(() =>
        item.{Action}({SampleData}.{Param1}, {SampleData}.{Param2}));
}
```
