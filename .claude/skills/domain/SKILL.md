---
model: claude-sonnet-4-6
description: Implement a complete domain feature in an HC.LIS module — business rules, domain events, aggregate changes, and unit tests following DDD conventions.
tools: Bash, Read, Write, Edit, Glob, Grep
---

# /domain

You implement complete domain features in HC.LIS modules. Each pattern (event, rule, ValueObject, entity, aggregate, test) has a dedicated template under `.claude/skills/domain/templates/`. **Read a template only when you are about to write that artifact** — don't preload them.

## Invocation
```
/domain [ModuleName?] <requirement description>
```

---

## Phase 1 — Resolve module
Extract `ModuleName` from the args (first PascalCase word).
- Not provided → list `src/HC.LIS/HC.LIS.Modules/` and ask the user to pick.
- Directory `src/HC.LIS/HC.LIS.Modules/{ModuleName}/` doesn't exist → stop with a clear error. Do not guess or create.

---

## Phase 2 — Explore the target module
Before writing anything, read enough of the target module to fit in (don't invent new patterns).

1. List `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Domain/` to identify aggregates, entities, events, rules.
2. Read the aggregate root file you'll be modifying.
3. Read child entity files involved in the change.
4. Skim existing events and rules in the same `{Aggregate}` folder for naming conventions.
5. Read `Tests/UnitTests/{Aggregate}/{Aggregate}Tests.cs`, the factory, and the sample-data file.

If the target module is sparse or inconsistent, fall back to the canonical module: **`TestOrders`** (`src/HC.LIS/HC.LIS.Modules/TestOrders/Domain/Orders/` and its `Tests/UnitTests/Orders/`).

---

## Phase 3 — Clarify if needed
Ask one focused question if any of these are ambiguous:
- Which aggregate or entity owns the behavior?
- Which state transitions / statuses are involved?
- What params does the command take?
- Which rules guard the transition?

Don't generate speculative code.

---

## Phase 4 — Implement

Pick artifacts from the table below. For each artifact you create, **read its template first** — the template has the file location, naming rules, full code shape, and the canonical reference file in `TestOrders`.

| Artifact | Read this template |
|---|---|
| New domain event | `.claude/skills/domain/templates/domain-event.md` |
| Business rule (exception + rule) | `.claude/skills/domain/templates/business-rule.md` |
| Status ValueObject | `.claude/skills/domain/templates/status-value-object.md` |
| Data-grouping ValueObject (e.g., `PatientInfo`) | `.claude/skills/domain/templates/value-object.md` |
| Entity with strongly-typed ID | `.claude/skills/domain/templates/entity.md` |
| Aggregate root command + `When()` | `.claude/skills/domain/templates/aggregate.md` |
| Unit test (happy path + broken rule) | `.claude/skills/domain/templates/unit-test.md` |

### Cross-cutting rules (always apply, no template lookup needed)
- **TDD:** write the failing test first, then implement just enough to pass. Test commits (`test:`) precede feature commits (`feat:`).
- **File-scoped namespaces** — no `{}` blocks around namespaces.
- **Private fields:** `_camelCase`. Static: `s_camelCase`. Constants: `PascalCase`. 4-space indent.
- **Domain events carry primitives only.** Unwrap ValueObjects (`.Value`, `.Prop`) at the call site.
- **ValueObjects** → private ctor + static `Of(...)` factory; reconstruct with `Of(...)`, never `new`.
- **Integration event handlers** that schedule a command live in the **same folder as the command**, not in a separate `Handle{Event}/` folder. File: `{Event}IntegrationEventHandler.cs`. Class: `{Event}IntegrationEventNotificationHandler` (CA1711 forbids the `EventHandler` type-name suffix).

### Analyzer rules (TreatWarningsAsErrors=true) — must not violate
- **CA1002** never expose `List<T>` — use `IReadOnlyCollection<T>`
- **CA1707** no underscores in public/test member names — PascalCase only
- **CA1716** avoid reserved keywords in namespaces
- **CA2007** `.ConfigureAwait(false)` on awaited tasks in infra/lib code
- **CA2201** specific exception types, never `new Exception()`

---

## Phase 5 — Verify
```bash
dotnet build src/HC.LIS/HC.LIS.Modules/{ModuleName}/Tests/UnitTests/HC.LIS.Modules.{ModuleName}.UnitTests.csproj
dotnet test  src/HC.LIS/HC.LIS.Modules/{ModuleName}/Tests/UnitTests/HC.LIS.Modules.{ModuleName}.UnitTests.csproj
```
Fix any compile or test failure before reporting success.

---

## HC.Core base classes — read on demand
These are stable contracts. Read only if you need to confirm a method signature.

| File | Provides |
|---|---|
| `src/HC.Core/Domain/Entity.cs` | `Events`, `ClearEvents()`, `AddEvent()`, `CheckRule()` |
| `src/HC.Core/Domain/EventSourcing/AggregateRoot.cs` | `AddDomainEvent()`, abstract `Apply()`, `Load()` |
| `src/HC.Core/Domain/ValueObject.cs` | reflection-based equality, `==`/`!=`, `CheckRule()` |
| `src/HC.Core/Domain/Id.cs` | base `Id` (throws on `Guid.Empty`, value-equality) |
| `src/HC.Core/Domain/IBusinessRule.cs` | rule interface |
| `src/HC.Core/Domain/BaseBusinessRuleException.cs` | required exception ctor overloads |
| `src/HC.Core/Domain/DomainEvent.cs` / `IDomainEvent.cs` | event base + marker |
| `src/HC.Core/Domain/SystemClock.cs` | `SystemClock.Now` |
| `src/HC.Core/Tests/UnitTests/TestBase.cs` | `AssertPublishedDomainEvent<T>()`, `AssertPublishedDomainEvents<T>()`, `AssertBrokenRule<TRule>()` |
