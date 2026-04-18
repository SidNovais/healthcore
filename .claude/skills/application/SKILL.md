---
model: claude-sonnet-4-6
description: Implement Application layer artifacts for an HC.LIS module — commands, queries, notifications, projectors, and internal commands following CQRS/DDD conventions.
tools: Bash, Read, Write, Edit, Glob, Grep
---

# /application

You implement Application layer artifacts in HC.LIS modules. Each artifact type has a dedicated template under `.claude/skills/application/templates/`. **Read a template only when you are about to write that artifact** — do not preload all templates.

## Invocation
```
/application [ModuleName?] <requirement description>
```

Examples:
- `/application TestOrders accept an exam`
- `/application Analyzer query analyzer sample details`
- `/application LabAnalysis when WorklistItemSigned occurs, publish integration event`

---

## Phase 1 — Resolve module

Extract `ModuleName` from the args (first PascalCase token).
- Not provided → list `src/HC.LIS/HC.LIS.Modules/` and ask the user to pick.
- Directory `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/` doesn't exist → stop with a clear error. Do not guess or create.

---

## Phase 2 — Explore the target module

Before writing anything:

1. List `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/` — identify aggregate folder names and existing use-case folders.
2. Read the existing use-case folder most similar to what you'll build (a CommandHandler, QueryHandler, or NotificationHandler).
3. Read the relevant domain aggregate (`Domain/{Aggregate}/{Aggregate}.cs`) — identify method names and parameter types.
4. If the requirement involves a domain event, read the `*DomainEvent.cs` to confirm property names.
5. If the requirement involves an integration event from another module, read that module's `IntegrationEvents/` to confirm constructor parameters.

If the target module has no existing use-case folders, fall back to the canonical module: **`TestOrders`** (`src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/`).

---

## Phase 3 — Clarify if needed

Ask **one focused question** if any of these are ambiguous:
- Which aggregate does the command target?
- Does the command return a value (`CommandBase<TResult>`) or not (`CommandBase`)?
- Which domain event triggers the notification chain?
- Does the notification need to publish an integration event, schedule an internal command, or both?
- What SQL table and column names does the query or projector read/write?

Do not generate speculative code.

---

## Phase 4 — Implement

Pick artifacts using the decision matrix below. For each artifact, **read its template first**.

### Decision matrix

| Requirement signal | Artifacts to generate | Templates to read |
|---|---|---|
| "handle command", aggregate load+mutate | Command + CommandHandler | `command.md` |
| "query", "get", "fetch", "list", "read" | Query + QueryHandler + DTO | `query.md` |
| "project to read model", "update projection" | NotificationProjection + Projector | `notification.md`, `projector.md` |
| "publish integration event", "notify other modules" | PublishEventNotificationHandler | `notification.md` |
| "on integration event, schedule command", "when [External]Event occurs" | InternalCommand + InternalCommandHandler + IntegrationEventNotificationHandler | `internal-command.md`, `command.md` |
| "domain event notification" (bridging DomainEvent to handlers) | Notification class | `notification.md` |

A complete write-side use case typically generates: Command + CommandHandler + Notification + NotificationProjection + Projector + PublishEventNotificationHandler.

### Folder placement

```
Application/{Aggregate}/{UseCaseName}/
├── {Action}Command.cs
├── {Action}CommandHandler.cs
├── {Event}Notification.cs
├── {Event}NotificationProjection.cs
├── {Event}PublishEventNotificationHandler.cs
├── {Action}By{Key}Command.cs                          ← internal command
├── {Action}By{Key}CommandHandler.cs
├── {ExternalEvent}IntegrationEventNotificationHandler.cs
│
└── (query folder — separate use case)
    Get{Aggregate}{Qualifier}/
    ├── Get{Aggregate}{Qualifier}Query.cs
    ├── Get{Aggregate}{Qualifier}QueryHandler.cs
    ├── {Aggregate}{Qualifier}Dto.cs
    └── {Aggregate}{Qualifier}Projector.cs              ← projector lives here
```

### Cross-cutting rules — NEVER VIOLATE

1. **`Handle()` is the only method in any handler.** No private helpers. No extracted methods. All logic inline.
2. **No SQL in CommandHandlers.** SQL belongs only in `QueryHandler.Handle()` and `Projector.When()`. If a CommandHandler needs read-model data, inject `IQueryHandler<TQuery, TResult>` and call `.Handle(new TQuery(...), cancellationToken)` inline.
3. **Handlers are `internal`.** `CommandHandler`, `QueryHandler`, and `{Aggregate}Projector` are all `internal`. `Notification`, `NotificationProjection`, `PublishEventNotificationHandler`, and `IntegrationEventNotificationHandler` are `public`.
4. **`notification.DomainEvent.X` inline** — never store `notification.DomainEvent` in a local variable.
5. **IntegrationEventNotificationHandler lives in the same folder as the command it schedules** — not in a separate handler folder.
6. **Projectors live in the query folder** (`Get{Aggregate}{Qualifier}/`), not in a separate `Projectors/` folder.
7. **`.ConfigureAwait(false)` on every awaited call** — CA2007 is a build error.
8. **File-scoped namespaces** — no curly-brace blocks.
9. **`IReadOnlyCollection<T>` for public collections** — CA1002 forbids `List<T>`.
10. **Projectors are auto-registered** by `DataAccessModule` scanning for `type.Name.EndsWith("Projector")`. Never edit DI registration files.

### Analyzer rules (TreatWarningsAsErrors=true)

| Rule | Requirement |
|---|---|
| CA1002 | `IReadOnlyCollection<T>`, never `List<T>` |
| CA1707 | No underscores in public member names |
| CA1711 | No class name ending in `EventHandler` — use `NotificationHandler` suffix |
| CA2007 | `.ConfigureAwait(false)` on all awaited tasks |
| CA2201 | No `new Exception()` — use `InvalidCommandException` or `InvalidOperationException` |

---

## Phase 5 — Verify

```bash
dotnet build src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/HC.LIS.Modules.{ModuleName}.Application.csproj
```

Fix any compile error before reporting success. Common issues:
- Missing `using` for `HC.Core.Application.Events` (`DomainNotificationBase`)
- Missing `using` for `HC.Core.Application.Projections` (`IProjector`, `ProjectorBase`)
- Missing `using` for `HC.Core.Infrastructure.EventBus` (`IEventsBus`)
- Namespace doesn't match folder path

---

## HC.Core base types — read on demand

| File | Provides |
|---|---|
| `src/HC.Core/Domain/EventSourcing/IAggregateStore.cs` | `Start<T>()`, `Load<T>()`, `AppendChanges<T>()` |
| `src/HC.Core/Application/Events/DomainNotificationBase.cs` | `DomainEvent`, `Id` |
| `src/HC.Core/Application/Projections/ProjectorBase.cs` | base `When(IDomainEvent)` fallback |
| `src/HC.Core/Application/Projections/IProjector.cs` | `Project(IDomainEvent)` |
| `src/HC.Core/Infrastructure/EventBus/IEventsBus.cs` | `Publish<T>(T)` |
| `src/HC.Core/Infrastructure/Data/ISqlConnectionFactory.cs` | `GetConnection()`, `CreateConnection()` |

---

## Critical reference files

| Purpose | File |
|---|---|
| Command + handler (load + mutate) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/AcceptExam/AcceptExamCommandHandler.cs` |
| Command + handler (create, returns Id) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/CreateOrder/CreateOrderCommandHandler.cs` |
| Query + handler (Dapper inline SQL + nameof) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/GetOrderDetailsQueryHandler.cs` |
| Notification (DomainNotificationBase subclass) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/AcceptExam/ExamAcceptedNotification.cs` |
| NotificationProjection (foreach projectors) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/AcceptExam/ExamAcceptedNotificationProjection.cs` |
| PublishEventNotificationHandler | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/RequestExam/ExamRequestedPublishEventNotificationHandler.cs` |
| InternalCommand ([method: JsonConstructor]) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/PlaceExamInProgress/PlaceExamInProgressByExamIdCommand.cs` |
| IntegrationEvent → schedule command handler | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/PlaceExamInProgress/SampleCollectedIntegrationEventHandler.cs` |
| Projector (single event, INSERT) | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/OrderDetailsProjector.cs` |
| Projector (multi-event with fallback) | `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/GetAnalyzerSampleDetails/AnalyzerSampleDetailsProjector.cs` |
| Handler injecting QueryHandler (no SQL, no helpers) | `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/AssignWorklistItem/AssignWorklistItemByBarcodeAndExamCodeCommandHandler.cs` |
