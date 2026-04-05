# Happy-path test templates

## When to use which variant

| Variant | Use when |
|---|---|
| **A — action in constructor** | The test class constructor already calls the method under test |
| **B — action in test body (shared `_sut`)** | Constructor creates `_sut` but does NOT call the new method |
| **C — per-test factory (LabAnalysis style)** | No shared `_sut`; each test builds its own state via factory |

---

## Variant A — action already called in shared constructor

```csharp
[Fact]
public void {Action}{Entity}IsSuccessful()
{
    {EventType} ev = AssertPublishedDomainEvent<{EventType}>(_sut);
    ev.{Prop1}.Should().Be({SampleData}.{Prop1});
    ev.{Prop2}.Should().Be({SampleData}.{Prop2});
    // one .Should().Be() per event property
}
```

---

## Variant B — action in test body, shared `_sut`

```csharp
[Fact]
public void {Action}{Entity}IsSuccessful()
{
    DateTime actionedAt = SystemClock.Now;
    _sut.{Action}{Entity}(new {EntityId}({SampleData}.{EntityIdField}), actionedAt);

    {EventType} ev = AssertPublishedDomainEvent<{EventType}>(_sut);
    ev.{EntityIdProp}.Should().Be({SampleData}.{EntityIdField});
    ev.{ActionedAtProp}.Should().Be(actionedAt);
    // one .Should().Be() per event property
}
```

---

## Variant C — per-test factory (no shared `_sut`)

```csharp
[Fact]
public void {Action}IsSuccessful()
{
    {Aggregate} item = {Aggregate}Factory.CreateWith{Prerequisite}();
    item.{Action}({SampleData}.{Param1}, {SampleData}.{Param2});

    {EventType} ev = AssertPublishedDomainEvent<{EventType}>(item);
    ev.{Prop1}.Should().Be({SampleData}.{Prop1});
    ev.{Prop2}.Should().Be({SampleData}.{Prop2});
    // one .Should().Be() per event property
}
```
