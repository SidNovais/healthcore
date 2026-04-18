# Query + QueryHandler + DTO — pattern reference

## When to read
Creating a read-side use case: a query object, a Dapper handler, and the DTO it returns. The projector that writes to the read-model table lives in the same folder — read `projector.md` if you also need to create one.

## File locations
```
Application/{Aggregate}/Get{Aggregate}{Qualifier}/Get{Aggregate}{Qualifier}Query.cs
Application/{Aggregate}/Get{Aggregate}{Qualifier}/Get{Aggregate}{Qualifier}QueryHandler.cs
Application/{Aggregate}/Get{Aggregate}{Qualifier}/{Aggregate}{Qualifier}Dto.cs
```

## Query
```csharp
using HC.LIS.Modules.{ModuleName}.Application.Contracts;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.Get{Aggregate}{Qualifier};

public class Get{Aggregate}{Qualifier}Query(
    Guid {aggregateId}
) : QueryBase<{Aggregate}{Qualifier}Dto?>
{
    public Guid {AggregateId} { get; } = {aggregateId};
}
```

## QueryHandler
```csharp
using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.{ModuleName}.Application.Configuration.Queries;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.Get{Aggregate}{Qualifier};

internal class Get{Aggregate}{Qualifier}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<Get{Aggregate}{Qualifier}Query, {Aggregate}{Qualifier}Dto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<{Aggregate}{Qualifier}Dto?> Handle(
        Get{Aggregate}{Qualifier}Query query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
            ""t"".""Id"" AS ""{nameof({Aggregate}{Qualifier}Dto.{AggregateId})}"",
            ""t"".""column_b"" AS ""{nameof({Aggregate}{Qualifier}Dto.PropertyB)}""
            FROM ""{schema_name}"".""TableName"" AS ""t""
            WHERE ""t"".""Id"" = @{AggregateId}";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Connection must exist to query {aggregate} {qualifier}");

        return await connection.QueryFirstOrDefaultAsync<{Aggregate}{Qualifier}Dto>(
            sql,
            new { query.{AggregateId} }
        ).ConfigureAwait(false);
    }
}
```

## DTO
```csharp
namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.Get{Aggregate}{Qualifier};

public class {Aggregate}{Qualifier}Dto
{
    public Guid {AggregateId} { get; set; }
    // ... other properties — must match the nameof() aliases in the SELECT
}
```

## SQL rules
- SQL is always **inline in `Handle()`** — no private method, no const field, no helper.
- Column aliases always use `nameof({Dto}.{Property})` — refactor-safe.
- For lists: use `QueryAsync<TDto>` instead of `QueryFirstOrDefaultAsync`.
- `GetConnection()` for ambient scoped connection (inside unit of work). `CreateConnection()` is for projectors only.

## IRON RULES
- `Handle()` is the **only method** in the QueryHandler.
- No domain aggregates loaded here. No `IAggregateStore`. Pure SQL → DTO.
- Handler is always `internal`.
- Every DTO property must have a `nameof()` alias in the SELECT.

## References
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/GetOrderDetailsQueryHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderItemDetails/GetOrderItemDetailsQueryHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/GetAnalyzerSampleDetails/GetAnalyzerSampleDetailsQueryHandler.cs`
