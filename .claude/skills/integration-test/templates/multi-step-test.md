# Multi-step test template

Use when the requirement describes behavior that can only be triggered after prior state is established through one or more preceding commands.

Each setup step follows the pattern: **dispatch command → poll to confirm state → proceed**.

---

## Full template

```csharp
[Fact]
public async Task {Action}{Entity}IsSuccessful()
{
    // Step 1 — establish initial state
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

    // Step 2 — transition to intermediate state
    await {ModuleName}Module.ExecuteCommandAsync(new {IntermediateAction}{Entity}Command(
        {SampleData}.{EntityId},
        {SampleData}.{IntermediateParam},
        SystemClock.Now
    )).ConfigureAwait(true);

    await GetEventually(
        new Get{Entity}DetailsFrom{ModuleName}Probe(
            {SampleData}.{EntityId},
            {ModuleName}Module,
            dto => dto?.Status == "{IntermediateStatus}"),
        15000
    ).ConfigureAwait(true);

    // Act — command under test
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
            dto => dto?.Status == "{FinalStatus}"),
        15000
    ).ConfigureAwait(true);

    details.Should().NotBeNull();
    details!.Status.Should().Be("{FinalStatus}");
    details.{AssertedProp}.Should().Be({SampleData}.{AssertedPropValue});
}
```

---

## Notes

- Each intermediate `GetEventually` is a **required synchronization barrier** — do not skip. Without it the next command may arrive before the read model is consistent.
- The predicate-based probe (`dto => dto?.Status == "..."`) avoids creating separate probe classes per intermediate state.
- Add as many setup steps as needed — the pattern repeats: dispatch → poll → dispatch → poll → act → poll → assert.
- **Discard intermediate poll results** (no variable assignment): `await GetEventually(...).ConfigureAwait(true);`
- Only the final `GetEventually` result is assigned: `{DtoType}? details = await GetEventually(...)`.
- `details!.` (non-null-forgiving) is only valid after `details.Should().NotBeNull()`.
- `SystemClock.Now` is valid for timestamps unless a specific fixed DateTime is required by the test.
