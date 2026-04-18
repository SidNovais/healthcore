# Command + CommandHandler — pattern reference

## When to read
Creating a regular command (mutates aggregate state) and its handler. Also use for internal command handlers — the handler shape is identical.

## File locations
```
Application/{Aggregate}/{UseCaseName}/{Action}Command.cs
Application/{Aggregate}/{UseCaseName}/{Action}CommandHandler.cs
```

## Naming
- Command: `{Action}Command` (e.g., `AcceptExamCommand`)
- Handler: `{Action}CommandHandler` — always `internal`
- Internal command by-key: `{Action}By{Key}Command` (e.g., `PlaceExamInProgressByExamIdCommand`)

## Command — no return value
```csharp
using HC.LIS.Modules.{ModuleName}.Application.Contracts;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

public class {Action}Command(
    Guid {aggregateId},
    // ... other params
) : CommandBase
{
    public Guid {AggregateId} { get; } = {aggregateId};
    // ... other properties
}
```

## Command — with return value
```csharp
using HC.LIS.Modules.{ModuleName}.Application.Contracts;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

public class {Action}Command(
    Guid {aggregateId},
    // ... other params
) : CommandBase<Guid>
{
    public Guid {AggregateId} { get; } = {aggregateId};
}
```

## CommandHandler — load existing aggregate
```csharp
using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.{ModuleName}.Application.Configuration.Commands;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

internal class {Action}CommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<{Action}Command>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        {Action}Command command,
        CancellationToken cancellationToken
    )
    {
        {Aggregate}? {aggregate} = await _aggregateStore.Load(new {Aggregate}Id(command.{AggregateId}))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("{Aggregate} must exist to {action description}");
        {aggregate}.{DomainMethod}(/* args from command */);
        _aggregateStore.AppendChanges({aggregate});
    }
}
```

## CommandHandler — create new aggregate (returns Id)
```csharp
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.{ModuleName}.Application.Configuration.Commands;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate};

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

internal class {Action}CommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<{Action}Command, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public Task<Guid> Handle(
        {Action}Command command,
        CancellationToken cancellationToken
    )
    {
        {Aggregate} {aggregate} = {Aggregate}.Create(
            command.{AggregateId}
            // ... map command props to domain value objects
        );
        _aggregateStore.Start({aggregate});
        return Task.FromResult({aggregate}.Id);
    }
}
```

## IRON RULES
- `Handle()` is the **only method**. No private helpers, no extracted methods, no `Task DoX(...)`.
- **No SQL here**. If read-model data is needed, inject `IQueryHandler<TQuery, TResult>` and call `.Handle(new TQuery(...), cancellationToken).ConfigureAwait(false)` inline.
- Handler is always `internal`.
- `.ConfigureAwait(false)` on every `await`.

## References
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/AcceptExam/AcceptExamCommandHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/CreateOrder/CreateOrderCommandHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/AssignWorklistItem/AssignWorklistItemByBarcodeAndExamCodeCommandHandler.cs`
