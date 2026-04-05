# Multiple events test template

Use when the description implies multiple events of the same type are raised (e.g., "record multiple analyte results").

Use `AssertPublishedDomainEvents<T>` (plural) and declare the result as `IReadOnlyCollection<T>` (CA1002).

```csharp
[Fact]
public void {Action}MultipleIsSuccessful()
{
    _sut.{Action}({args1});
    _sut.{Action}({args2});

    IReadOnlyCollection<{EventType}> events = AssertPublishedDomainEvents<{EventType}>(_sut);
    events.Should().HaveCount(2);
    events.Should().Contain(e => e.{DiscriminatorProp} == {value1});
    events.Should().Contain(e => e.{DiscriminatorProp} == {value2});
}
```

For per-test factory style:

```csharp
[Fact]
public void {Action}MultipleIsSuccessful()
{
    {Aggregate} item = {Aggregate}Factory.CreateWith{Prerequisite}();
    item.{Action}({args1});
    item.{Action}({args2});

    IReadOnlyCollection<{EventType}> events = AssertPublishedDomainEvents<{EventType}>(item);
    events.Should().HaveCount(2);
    events.Should().Contain(e => e.{DiscriminatorProp} == {value1});
    events.Should().Contain(e => e.{DiscriminatorProp} == {value2});
}
```
