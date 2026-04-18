# Projector — pattern reference

## When to read
Creating a class that receives domain events via `IProjector.Project()` and writes INSERT/UPDATE SQL to the read-model table. Projectors always live in the **same folder as the query they feed**.

## File location
```
Application/{Aggregate}/Get{Aggregate}{Qualifier}/{Aggregate}{Qualifier}Projector.cs
```

## Naming
- Class: `{Aggregate}{Qualifier}Projector` — e.g., `OrderDetailsProjector`
- Extends `ProjectorBase`, implements `IProjector`
- Class is `internal`

## Projector template
```csharp
using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Events;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.Get{Aggregate}{Qualifier};

internal class {Aggregate}{Qualifier}Projector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When({Event1}DomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO {schema_name}.""TableName""
              (""Id"", ""Column2"", ""Column3"")
              VALUES (@Prop1, @Prop2, @Prop3)",
            new { e.Prop1, e.Prop2, e.Prop3 }
        ).ConfigureAwait(false);
    }

    private async Task When({Event2}DomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE {schema_name}.""TableName""
              SET ""Column2"" = @Prop2
              WHERE ""Id"" = @Prop1",
            new { e.Prop1, e.Prop2 }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
```

## Key structural rules
- `Project()` always dispatches via `When((dynamic)@event)` — this is the dynamic dispatch pattern.
- One `private async Task When({SpecificEventType} e)` per domain event handled.
- The fallback `private static new Task When(IDomainEvent _) => Task.CompletedTask;` is **always the last method** — silences unhandled event types.
- Use `_sqlConnectionFactory.CreateConnection()` with `using var` — projectors manage their own connection lifetime (not inside a unit of work).
- SQL is always **inline in each `When()` overload** — no private SQL method.

## DI auto-registration
`DataAccessModule` scans for `type.Name.EndsWith("Projector", StringComparison.Ordinal)` and registers as `IProjector`. **Never edit DI registration files when adding a Projector.**

## IRON RULES
- Class is `internal`.
- `Project()` dispatches via `When((dynamic)@event)` — never switch on type, never cast.
- Fallback `When(IDomainEvent _)` is always present and always last.
- Use `CreateConnection()`, not `GetConnection()`.
- `.ConfigureAwait(false)` on every `await`.
- No private helper methods — one `When()` per event type, SQL inline.

## References
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/OrderDetailsProjector.cs`
- `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/GetAnalyzerSampleDetails/AnalyzerSampleDetailsProjector.cs`
- `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/GetAnalyzerSampleExamDetails/AnalyzerSampleExamDetailsProjector.cs`
