# InternalCommand (InternalCommandBase) — pattern reference

## When to read
Creating a command that is dispatched through the outbox/scheduler pipeline (not directly from an API endpoint). Internal commands are always triggered by an integration event from another module or a domain notification.

## File locations
```
Application/{Aggregate}/{UseCaseName}/{Action}By{Key}Command.cs
Application/{Aggregate}/{UseCaseName}/{Action}By{Key}CommandHandler.cs
Application/{Aggregate}/{UseCaseName}/{ExternalEvent}IntegrationEventNotificationHandler.cs  ← same folder
```

## Naming
- Class: `{Action}By{Key}Command` — e.g., `PlaceExamInProgressByExamIdCommand`
- `[method: JsonConstructor]` is **mandatory** — required for outbox deserialization.
- `Guid id` must be the **first** constructor parameter.

## InternalCommand
```csharp
using System.Text.Json.Serialization;
using HC.LIS.Modules.{ModuleName}.Application.Configuration.Commands;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

[method: JsonConstructor]
public class {Action}By{Key}Command(
    Guid id,
    Guid {aggregateId},
    // ... other params
) : InternalCommandBase(id)
{
    public Guid {AggregateId} { get; } = {aggregateId};
    // ... other properties
}
```

## Handler shape
The handler is identical to a regular CommandHandler — read `command.md`. Class is `internal`. The only structural difference is the command class itself extends `InternalCommandBase` instead of `CommandBase`.

## IRON RULES
- `[method: JsonConstructor]` is **mandatory** — omitting it silently breaks outbox deserialization.
- `Guid id` must be the **first** parameter.
- Extend `InternalCommandBase(id)`, not `CommandBase`.
- Do NOT add `ICommand` directly — `InternalCommandBase` already implements it.
- The `IntegrationEventNotificationHandler` that enqueues this command lives in **this same folder**.

## References
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/PlaceExamInProgress/PlaceExamInProgressByExamIdCommand.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/PlaceExamInProgress/PlaceExamInProgressByExamIdCommandHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/CompleteExam/CompleteExamByExamIdCommand.cs`
- `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/CreateAnalyzerSample/CreateAnalyzerSampleBySampleCollectedCommand.cs`
