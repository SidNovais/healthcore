# Probe class templates

A probe is polled repeatedly by `GetEventually` until `IsSatisfied` returns `true` or the timeout expires. Choose the variant that best matches the module's existing style.

---

## Variant A — with optional predicate parameter (LabAnalysis style)

Use when the same DTO will be polled at **multiple states** in the same test file. The `satisfiedWhen` parameter lets callers specify the acceptance condition inline — avoids creating separate probe classes per state.

```csharp
using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Application.{Aggregate}s.Get{Aggregate}Details;

namespace HC.LIS.Modules.{ModuleName}.IntegrationTests.{Aggregate}s;

public class Get{Aggregate}DetailsFrom{ModuleName}Probe(
    Guid expected{Aggregate}Id,
    I{ModuleName}Module {moduleName}Module,
    Func<{Aggregate}DetailsDto?, bool>? satisfiedWhen = null
) : IProbe<{Aggregate}DetailsDto>
{
    private readonly Guid _expected{Aggregate}Id = expected{Aggregate}Id;
    private readonly I{ModuleName}Module _{moduleName}Module = {moduleName}Module;
    private readonly Func<{Aggregate}DetailsDto?, bool> _satisfiedWhen = satisfiedWhen ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"{Aggregate}Details not found or unsatisfied for {_expected{Aggregate}Id}";

    public async Task<{Aggregate}DetailsDto?> GetSampleAsync()
    {
        return await _{moduleName}Module
            .ExecuteQueryAsync(new Get{Aggregate}DetailsQuery(_expected{Aggregate}Id))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied({Aggregate}DetailsDto? sample) => _satisfiedWhen(sample);
}
```

---

## Variant B — fixed `IsSatisfied` (TestOrders style)

Use when the probe targets **one specific state**. Create a separate class per state when multiple polling states are needed.

```csharp
using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Application.{Aggregate}s.Get{Aggregate}Details;

namespace HC.LIS.Modules.{ModuleName}.IntegrationTests.{Aggregate}s;

public class Get{Aggregate}{State}From{ModuleName}Probe(
    Guid expected{Aggregate}Id,
    I{ModuleName}Module {moduleName}Module
) : IProbe<{Aggregate}DetailsDto>
{
    private readonly Guid _expected{Aggregate}Id = expected{Aggregate}Id;
    private readonly I{ModuleName}Module _{moduleName}Module = {moduleName}Module;

    public string DescribeFailureTo() =>
        $"{Aggregate} {_expected{Aggregate}Id} did not reach {State} status";

    public async Task<{Aggregate}DetailsDto?> GetSampleAsync()
    {
        return await _{moduleName}Module
            .ExecuteQueryAsync(new Get{Aggregate}DetailsQuery(_expected{Aggregate}Id))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied({Aggregate}DetailsDto? sample) =>
        sample?.Status == "{State}";
}
```

---

## Variant C — DB-direct probe (no module facade query)

Use as a **fallback** when the `Get{Aggregate}DetailsQuery` does not yet exist (because the application layer hasn't been implemented). Queries the read-model table directly via Dapper so the test file compiles even before the query handler exists.

```csharp
using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using HC.Core.IntegrationTests.Probing;

namespace HC.LIS.Modules.{ModuleName}.IntegrationTests.{Aggregate}s;

public class Get{Aggregate}ByIdProbe(
    Guid expected{Aggregate}Id,
    string? connectionString
) : IProbe<{Aggregate}ReadModel>
{
    private readonly Guid _expected{Aggregate}Id = expected{Aggregate}Id;
    private readonly string? _connectionString = connectionString;

    public string DescribeFailureTo() =>
        $"{Aggregate} not found for Id {_expected{Aggregate}Id}";

    public async Task<{Aggregate}ReadModel?> GetSampleAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<{Aggregate}ReadModel>(
            @"SELECT ""{IdColumn}"" AS ""{Aggregate}Id"", ""{Prop1}"", ""{Prop2}"", ""Status""
              FROM ""{module_schema}"":""{ReadModelTable}""
              WHERE ""{IdColumn}"" = @Id",
            new { Id = _expected{Aggregate}Id }
        ).ConfigureAwait(false);
    }

    public bool IsSatisfied({Aggregate}ReadModel? sample) => sample is not null;
}
```

> When using Variant C, define `{Aggregate}ReadModel` as a simple record in the same file:
> ```csharp
> internal record {Aggregate}ReadModel(Guid {Aggregate}Id, string Status, /* other props */);
> ```

---

## Notes

- All probe classes use **primary constructor syntax** (C# 12).
- `GetSampleAsync()` always uses `.ConfigureAwait(false)` (probe is infrastructure-layer code, not test code).
- `DescribeFailureTo()` must include the entity ID for actionable failure output.
- **Choose Variant A** when the module follows the LabAnalysis pattern (multi-state polling in the same file). **Choose Variant B** when each test targets one status transition. **Choose Variant C** only when the query handler doesn't exist yet.
- Replace `{Aggregate}`, `{ModuleName}`, `{moduleName}` (camelCase for parameter/field names), `{State}`, `{module_schema}`, `{ReadModelTable}`, `{IdColumn}` with actual values.
