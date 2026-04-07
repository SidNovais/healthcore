# Inbox injection test template

Use when the requirement describes behavior triggered by an integration event published by another module, consumed via the InboxMessages table (Quartz inbox processor).

The test manually inserts a serialized event record into `"{module_schema}"."InboxMessages"`, then polls for the expected read-model change.

---

## Template

```csharp
[Fact]
public async Task {Action}{Entity}Via{EventName}IsSuccessful()
{
    // Prerequisite state — omit if the event handler creates the aggregate from scratch
    await {ModuleName}Module.ExecuteCommandAsync(new {CreateEntity}Command(
        {SampleData}.{EntityId},
        {SampleData}.{Param1},
        SystemClock.Now
    )).ConfigureAwait(true);

    await GetEventually(
        new Get{Entity}DetailsFrom{ModuleName}Probe({SampleData}.{EntityId}, {ModuleName}Module),
        15000
    ).ConfigureAwait(true);

    // Inject integration event into inbox
    var integrationEvent = new {EventName}(
        Guid.CreateVersion7(),
        SystemClock.Now,
        {SampleData}.{Prop1},
        {SampleData}.{Prop2}
    );

    using (var connection = new NpgsqlConnection(ConnectionString))
    {
        string? type = integrationEvent.GetType().FullName;
        string data = JsonConvert.SerializeObject(integrationEvent, new JsonSerializerSettings
        {
            ContractResolver = new AllPropertiesContractResolver()
        });
        await connection.ExecuteScalarAsync(
            @"INSERT INTO ""{module_schema}"".""InboxMessages"" (""Id"", ""OccurredAt"", ""Type"", ""Data"") VALUES (@Id, @OccurredAt, @Type, @Data)",
            new { integrationEvent.Id, integrationEvent.OccurredAt, type, data }
        ).ConfigureAwait(true);
    }

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

## Required using directives (add only those not already present in the file)

```csharp
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using HC.Core.Infrastructure.Serialization;
using HC.LIS.Modules.{SourceModule}.IntegrationEvents;
```

---

## Notes

- `{module_schema}` is the SQL schema string found in `TestBase.ClearDatabase()` for this module (e.g., `test_orders`, `lab_analysis`). Use the exact string — it is case-sensitive.
- `AllPropertiesContractResolver` is in `HC.Core.Infrastructure.Serialization` — always use it for serializing integration events to match the production serializer.
- The `using` block around `NpgsqlConnection` is required.
- `integrationEvent.Id` and `integrationEvent.OccurredAt` are inherited from the `IntegrationEvent` base class — always use them as `@Id` and `@OccurredAt` parameters.
- Use `Guid.CreateVersion7()` for the event `Id`. Use `SystemClock.Now` for `OccurredAt`.
- After injection, **always poll with a predicate** that checks the specific expected status (`dto?.Status == "..."`), not just `dto is not null`, so the test waits until the inbox handler has processed the event.
- When the test class has a shared constructor that already establishes prerequisite state, omit the setup steps from the test body.
- When no prior aggregate state is needed (the event handler creates the aggregate), omit all setup and intermediate poll steps — start directly at the inbox injection block.
