# Observability Handoff — Serilog + OpenTelemetry

**Date:** 2026-06-28  
**Status:** Plan approved, not yet implemented.  
**Full plan:** `C:\Users\sidne\.claude\plans\let-s-create-a-tech-imperative-pearl.md`

---

## What This Implements

Full system observability across all 6 business modules, HC.LIS.API, HC.LIS.TcpMessage, and HC.LIS.Database:

- **Structured logs** — Serilog enriched with `TraceId`/`SpanId` via `Serilog.Enrichers.Span` so every log line is correlated to its trace.
- **Distributed traces** — OpenTelemetry `ActivitySource` per module. A generic `TracingCommandHandlerDecorator<T>` in `HC.Core.Infrastructure.Observability` wraps every command and query as a span. Registered as outermost Autofac decorator in each module's `ProcessingModule`.
- **Async trace propagation** — `Activity.Current?.Id` (W3C traceparent) is stored to the database when:
  - An InternalCommand is enqueued via `CommandsScheduler.EnqueueAsync()`
  - A domain event reaches the Outbox via `DomainEventsDispatcher`
  - An integration event arrives in the Inbox via `IntegrationEventGenericHandler`
  When Quartz picks those rows up, `ActivityContext.TryParse` restores the parent context before execution — spans link back across job boundaries.
- **Full chain guarantee** — the Inbox→InternalCommand leg is explicitly handled: the restored InboxMessage span stays open (`using var activity`) during `_mediator.Publish()`, so any `ICommandsScheduler.EnqueueAsync()` called inside the notification handler captures the right `Activity.Current?.Id`.
- **Custom metrics** — `HcMeter` (in `HC.Core.Infrastructure.Observability`) exposes `hclis.commands.executed` (counter) and `hclis.commands.duration_ms` (histogram) via `System.Diagnostics.Metrics` (BCL, no extra package).
- **OTLP export** — controlled by `ASPNETCORE_HCLIS_OTEL_ENDPOINT` env var. Falls back to console export when unset (good for local dev).

---

## Key Design Decisions

| Decision | Why |
|---|---|
| Decorator in `HC.Core`, registered against `IRequestHandler<>` not `ICommandHandler<>` | HC.Core doesn't know module-specific `ICommandHandler<T>`. Since `ICommandHandler<T> : IRequestHandler<T>`, registering against `IRequestHandler<>` stacks correctly on top of Logging/Validation/UoW decorators. |
| `ActivitySource` as singleton per module's Autofac container | Each bounded context has its own named source (`"HC.LIS.TestOrders"` etc.). The OTel SDK at the API/Worker host level listens globally via `ActivityListener`, so module-internal Autofac containers don't need to reference the SDK directly. |
| Store W3C traceparent (`Activity.Current?.Id`), not just TraceId | Storing the full traceparent (e.g. `00-4bf92f…-00f067…-01`) allows `ActivityContext.TryParse` to restore both TraceId and SpanId, making job spans true children of the original span rather than orphaned continuations. |
| `using var activity` scope in Outbox/Inbox handlers | Ensures `Activity.Current` is set during the entire `_mediator.Publish()` call, so nested handlers that enqueue further InternalCommands see the correct parent. |

---

## Task Summary (11 tasks)

| # | Task | Scope |
|---|---|---|
| 1 | NuGet packages | `Directory.Packages.props` + 3 csproj files |
| 2 | HC.Core tracing decorators + HcMeter + unit tests | 5 files in `HC.Core` |
| 3 | TraceContext on OutboxMessage, InboxMessage, DomainEventsDispatcher | 3 files in `HC.Core` |
| 4 | Database migrations (TraceContext column) | 6 new migration files |
| 5 | CommandsScheduler capture TraceContext on enqueue | 6 module files (×2 methods each) |
| 6 | SqlOutboxAccessor + IntegrationEventGenericHandler persist TraceContext | 12 module files |
| 7 | ObservabilityModule (ActivitySource) + ProcessingModule decorator registration | 18 module files |
| 8 | Job handlers restore TraceContext (InternalCommands, Outbox, Inbox) | 15 module files |
| 9 | HC.LIS.API OTel + Serilog.Span | 1 file |
| 10 | HC.LIS.TcpMessage OTel + Serilog.Span | 1 file |
| 11 | HC.LIS.Database Serilog.Span | 1 file |

---

## Things to Review Before Starting

1. **NuGet versions** — the plan pins `OpenTelemetry 1.11.2`. Verify against [nuget.org](https://www.nuget.org/packages/OpenTelemetry) that this is available for .NET 10. The `EntityFrameworkCore` and `Quartz` instrumentations are still in beta (`1.0.0-beta.12`, `1.0.0-beta.4`) — check if newer stable versions exist.
2. **`ActivityContext.TryParse` signature** — available in .NET 7+ BCL. Confirmed compatible with .NET 10.
3. **Serilog output template** — once Serilog.Enrichers.Span is wired, add `{TraceId}` to the console template in both Program.cs files if you want trace IDs visible in plain log output.
4. **No OTel collector in dev** — the plan uses console exporter as default. If you want Jaeger/Grafana Tempo locally, add a service to `development-compose.yaml` and set `ASPNETCORE_HCLIS_OTEL_ENDPOINT=http://localhost:4317`.

---

## How to Proceed

```bash
# Option A: Subagent-driven (recommended — review between tasks)
/subagent-driven-development

# Option B: Inline execution with checkpoints
/executing-plans
```

Point the chosen skill at: `C:\Users\sidne\.claude\plans\let-s-create-a-tech-imperative-pearl.md`
