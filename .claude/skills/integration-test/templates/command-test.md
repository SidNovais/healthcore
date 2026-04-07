# Command test template (happy path, single step)

Use when the requirement describes a single new command dispatched through the module facade, and the aggregate does not yet exist at test start.

---

## Variant A — self-contained test (no shared constructor setup)

Use when creating a new test class (empty constructor) or when adding a test that is fully self-contained.

```csharp
[Fact]
public async Task {Action}{Entity}IsSuccessful()
{
    // Arrange — create aggregate via root command
    await {ModuleName}Module.ExecuteCommandAsync(new {CreateEntity}Command(
        {SampleData}.{EntityId},
        {SampleData}.{Param1},
        {SampleData}.{Param2},
        SystemClock.Now
    )).ConfigureAwait(true);

    await GetEventually(
        new Get{Entity}DetailsFrom{ModuleName}Probe({SampleData}.{EntityId}, {ModuleName}Module),
        15000
    ).ConfigureAwait(true);

    // Act
    await {ModuleName}Module.ExecuteCommandAsync(new {Action}{Entity}Command(
        {SampleData}.{EntityId},
        {SampleData}.{ActionParam},
        SystemClock.Now
    )).ConfigureAwait(true);

    // Assert
    {EntityDetailsDto}? details = await GetEventually(
        new Get{Entity}DetailsFrom{ModuleName}Probe(
            {SampleData}.{EntityId},
            {ModuleName}Module,
            dto => dto?.Status == "{ExpectedStatus}"),
        15000
    ).ConfigureAwait(true);

    details.Should().NotBeNull();
    details!.Status.Should().Be("{ExpectedStatus}");
    details.{AssertedProp}.Should().Be({SampleData}.{AssertedPropValue});
}
```

---

## Variant B — test added to a class with shared constructor setup

Use when the test class constructor already dispatches the create command and polls for the aggregate. The `[Fact]` method dispatches only the new action command.

```csharp
[Fact]
public async void {Action}{Entity}IsSuccessfully()
{
    await {ModuleName}Module.ExecuteCommandAsync(new {Action}{Entity}Command(
        {SampleData}.{EntityId},
        {SampleData}.{ActionParam},
        SystemClock.Now
    )).ConfigureAwait(true);

    {EntityDetailsDto}? details = await GetEventually(
        new Get{Entity}DetailsFrom{ModuleName}Probe(
            {SampleData}.{EntityId},
            {ModuleName}Module),
        15000
    ).ConfigureAwait(true);

    details?.Status.Should().Be("{ExpectedStatus}");
    details?.{AssertedProp}.Should().Be({SampleData}.{AssertedPropValue});
    details?.{TimestampProp}.Should().NotBeNull();
}
```

---

## Notes

- The first `GetEventually` after the create command is a **synchronization barrier** — wait for the read model to be populated before dispatching the next command.
- When using the predicate probe (Variant A probe), pass `dto => dto?.Status == "{ExpectedStatus}"` to the final `GetEventually` so it waits for the correct status, not just any result.
- `details!.` (non-null-forgiving) is only valid after `details.Should().NotBeNull()`.
- Assert every DTO property that the new command sets, plus `Status`. Do NOT assert properties unchanged by this command.
- Match `async void` vs `async Task` to the existing test class style. Default to `async Task` for new classes.
