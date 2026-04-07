# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**HealthCore .NET** is a production-like ASP.NET Core application simulating a medical laboratory platform (LIS/RIS). It is a **Modular Monolith** reference implementation following the [ardalis/modular-monolith-with-ddd](https://github.com/ardalis/modular-monolith-with-ddd) structure.

**Stack:** .NET 10, C# 13, PostgreSQL, Marten (event store), EF Core, Autofac, MediatR, RabbitMQ (optional), Quartz.NET, Serilog, xUnit, FluentAssertions, NSubstitute, NetArchTest.Rules

---

## Commands

### Build

```bash
dotnet build
```

Build configuration in `Directory.Build.props`: `TreatWarningsAsErrors=true`, nullable enabled, all analyzers on.

### Test

```bash
# Run a specific test project
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/HC.LIS.Modules.TestOrders.UnitTests.csproj
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/ArchTests/HC.LIS.Modules.TestOrders.ArchTests.csproj

# Run a single test by name
dotnet test --filter "FullyQualifiedName~CreateOrderIsSuccessful" src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/HC.LIS.Modules.TestOrders.UnitTests.csproj
```

### Database

```bash
# Start PostgreSQL via Docker (localhost:5432, DB: Healthcore.Dev, user: dev/dev)
docker-compose -f development-compose.yaml up -d

# Run FluentMigrator migrations
dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj
```

The database connection string is read from the environment variable `ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING`.

---

## Architecture

### Modular Monolith with Clean Architecture

There is **no `.sln` file**. Projects are linked via `Directory.Build.targets` using naming conventions.

```
src/
├── HC.Core/                         # Shared kernel — no business logic
│   ├── Domain/                      # Base abstractions: Entity, ValueObject, AggregateRoot, IDomainEvent, IBusinessRule
│   ├── Application/                 # DomainEventsDispatcher, ProjectorBase, IExecutionContextAccessor
│   └── Infrastructure/              # IEventsBus, IOutbox, InboxMessage, DomainEventsDispatcherNotificationHandlerDecorator
│
└── HC.LIS/                          # Laboratory Information System
    ├── HC.LIS.Database/             # FluentMigrator migrations (runs as standalone tool)
    └── HC.LIS.Modules/
        └── TestOrders/              # Module: manages lab order lifecycle
            ├── Domain/
            ├── Application/
            ├── Infrastructure/
            ├── IntegrationEvents/   # External event contracts (JSON-serializable)
            └── Tests/
                ├── UnitTests/
                ├── IntegrationTests/
                └── ArchTests/
```

### Layer Rules (enforced by ArchTests via NetArchTest.Rules)

- `Domain` → no external dependencies
- `Application` → depends only on `Domain`
- `Infrastructure` → depends on `Application` and `Domain`

**Never violate layer boundaries in code:**

- SQL queries belong in **query handlers** (`IQueryHandler<,>`) or **domain-defined provider interfaces** implemented in Infrastructure — never inline inside command handlers.
- AWS SDK, EF Core, Dapper, and all third-party infrastructure clients belong in **Infrastructure** — never in Application or Domain.
- Command handlers must not contain `ISqlConnectionFactory`, `IDbConnection`, or any raw SQL. If a handler needs read-model data, it should either inject the relevant `IQueryHandler<,>` directly (same DI scope) or resolve a domain-defined provider interface (e.g., `IWorklistItemForSigningProvider`).
- Do not create `CLAUDE.md` files inside individual modules or layers — all architecture guidance belongs in the root `CLAUDE.md`.

**Domain-defined provider interfaces** — when a domain factory or aggregate method needs data that only Infrastructure can supply (e.g., a DB query required to enforce a business rule):

1. Define a value object in `Domain` carrying exactly what the domain needs.
2. Define the provider interface in `Domain` (e.g., `IWorklistItemForSigningProvider`).
3. Implement the interface in `Infrastructure` using Dapper.
4. Inject the interface into the Application command handler; pass the result into the aggregate method.

### Key Architectural Patterns

- **Event Sourcing** — aggregates state is rebuilt by replaying domain events (via Marten)
- **CQRS** — commands mutate state, queries read state; each command has a paired handler
- **Transactional Outbox + Inbox** — domain events are persisted atomically with the aggregate; Quartz.NET jobs relay them to the event bus
- **Business Rules as objects** — each invariant is its own class implementing `IBusinessRule`
- **MediatR pipeline decorators** — handle cross-cutting concerns (validation, logging, unit of work)
- **Autofac modules** — DI composition split into named modules (`DataAccessModule`, `OutboxModule`, `QuartzModule`, etc.) initialized via `TestOrdersStartup`

The public API of each module is a single facade class (e.g., `TestOrdersModule` implementing `ITestOrdersModule`).

---

## Conventions

### Naming

| Concept | Example |
|---|---|
| Domain Event | `OrderItemAcceptedDomainEvent` |
| Command + Handler | `AcceptExamCommand` + `AcceptExamCommandHandler` |
| Internal Command + Handler | `PlaceExamInProgressByExamIdCommand` + `PlaceExamInProgressByExamIdCommandHandler` |
| Notification | `ExamAcceptedNotification` |
| Business Rule | `CannotAcceptOrderItemMoreThanOnceRule` |
| Integration Event | `OrderItemAcceptedIntegrationEvent` |
| Integration Event Handler | file: `SampleCollectedIntegrationEventHandler.cs` · class: `SampleCollectedIntegrationEventNotificationHandler` |

> **Never** use `InternalCommand` as a suffix on class names. Internal commands (those extending `InternalCommandBase`) must be named to convey their specific purpose — typically describing *what* they do and *by what identifier* (e.g., `PlaceExamInProgressByExamIdCommand`, not `PlaceExamInProgressInternalCommand`).

### Code Style (.editorconfig)

- 4 spaces indentation
- Private fields: `_camelCase`, static fields: `s_camelCase`, constants: `PascalCase`

### Testing Pattern

Tests follow **Arrange–Act–Assert** with FluentAssertions. `TestBase` provides `AssertPublishedDomainEvent<T>()` to verify events raised on aggregates. `OrderSampleData` holds shared test data; `OrderFactory` builds test aggregates.

**TDD is required.** Always write the failing test first, then implement only enough code to make it pass. Test commits (`test:`) must precede the corresponding feature commits (`feat:`). Never write production code without a failing test driving it.

---

## Adding a New Feature

Typical flow for a new exam lifecycle command:

1. **Tests** — write a failing unit test in `OrderTests.cs` asserting the expected domain event via `AssertPublishedDomainEvent<T>()`
2. **Domain** — add business rule(s) in `Rules/`, domain event in `Events/`, method on the aggregate
3. **Application** — add `*Command`, `*CommandHandler`, `*Notification` in a subfolder under `Application/Orders/`
4. **IntegrationEvents** — add `*IntegrationEvent` if external systems need notification
