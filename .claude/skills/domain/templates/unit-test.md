# Unit test — pattern reference

## When to read
You are writing unit tests for a new aggregate command or business rule. TDD: write the failing test **first**, then implement.

## File location
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Tests/UnitTests/{Aggregate}/{Aggregate}Tests.cs` (existing file — append, do not create a new one).

## Rules
- Use `xUnit` `[Fact]` + `FluentAssertions`.
- Test class inherits `TestBase` (HC.Core unit tests) for `AssertPublishedDomainEvent<T>()`, `AssertPublishedDomainEvents<T>()`, `AssertBrokenRule<TRule>()`.
- Use the existing `{Aggregate}Factory.Create()` to build the aggregate; never construct it inline.
- Use `{Aggregate}SampleData` for shared constants (Guids, strings, dates).
- Use `SystemClock.Now` instead of `DateTime.UtcNow`.
- **Test naming (CA1707-strict):** PascalCase only, no underscores.
  - Happy path: `{Action}{Concept}IsSuccessful`
  - Broken rule: `{Action}{Concept}ShouldBroke{RuleName}When{Condition}`

## Template

### Happy path
```csharp
[Fact]
public void {Action}{Concept}IsSuccessful()
{
    {Aggregate} sut = {Aggregate}Factory.Create();
    DateTime actionedAt = SystemClock.Now;

    sut.{Action}(new {Entity}Id({Aggregate}SampleData.{Entity}Id), actionedAt);

    {EventType} ev = AssertPublishedDomainEvent<{EventType}>(sut);
    ev.{Entity}Id.Should().Be({Aggregate}SampleData.{Entity}Id);
    ev.{ActionedAt}.Should().Be(actionedAt);
}
```

### Broken-rule (one [Fact] per rule)
```csharp
[Fact]
public void {Action}{Concept}ShouldBroke{RuleName}When{Condition}()
{
    {Aggregate} sut = {Aggregate}Factory.Create();
    // put the aggregate into the state that triggers the rule:
    sut.{Action}(new {Entity}Id({Aggregate}SampleData.{Entity}Id), SystemClock.Now);

    void action()
    {
        sut.{Action}(new {Entity}Id({Aggregate}SampleData.{Entity}Id), SystemClock.Now);
    }

    AssertBrokenRule<Cannot{X}Rule>(action);
}
```

## Multi-event assertion
For tests that need to verify a sequence of events (e.g., "the last event sets a flag"), use `AssertPublishedDomainEvents<T>(sut)` (plural) and inspect `.Last()` / `.HaveCount(n)`.

## Reference
- `src/HC.Core/Tests/UnitTests/TestBase.cs` — `AssertPublishedDomainEvent<T>()`, `AssertPublishedDomainEvents<T>()`, `AssertBrokenRule<TRule>()`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/Orders/OrderTests.cs` — happy-path + broken-rule examples
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/Orders/OrderFactory.cs` — test factory
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/Orders/OrderSampleData.cs` — shared constants
- `src/HC.Core/Domain/SystemClock.cs` — `SystemClock.Now`
