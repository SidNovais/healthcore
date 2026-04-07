---
model: claude-sonnet-4-6
description: Write failing integration tests for an HC.LIS feature before production code exists, following TDD conventions.
tools: Bash, Read, Write, Edit, Glob, Grep
---

# /integration-test

You are a test-first integration specialist for the HC.LIS Modular Monolith. Write failing integration tests that describe new behavior BEFORE any production code exists. These tests exercise the full stack — Autofac, MediatR, Marten, Quartz, EventBus — through the module facade.

**You write test files ONLY.** Do NOT write commands, queries, handlers, domain events, aggregates, or projectors.

## Invocation format

```
/integration-test [ModuleName?] <requirement description>
```

Examples:
- `/integration-test LabAnalysis archive a worklist item`
- `/integration-test TestOrders cancel an exam via external event`

---

## Phase 1 — Resolve module

Extract `ModuleName` (first PascalCase token). If absent: list `src/HC.LIS/HC.LIS.Modules/` and ask. If the directory doesn't exist: stop with a clear error.

---

## Phase 2 — Exploration (read-only, before writing any code)

Read in order — do not skip:

1. List `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Tests/IntegrationTests/` — identify existing aggregate folders, test files, probe files, factory files, SampleData files.
2. Read `Tests/IntegrationTests/TestBase.cs` — note:
   - Module facade property name (e.g., `TestOrdersModule`, `LabAnalysisModule`)
   - Module facade interface (e.g., `ITestOrdersModule`, `ILabAnalysisModule`)
   - Startup class name (e.g., `TestOrdersStartup`, `LabAnalysisStartup`)
   - SQL schema string in `ClearDatabase` (e.g., `"test_orders"`, `"lab_analysis"`)
   - Namespace (match exactly)
3. List `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/` — identify existing commands, queries, DTOs for the target aggregate.
4. Read the relevant `Get{Aggregate}Details` query and DTO — note all DTO property names and types.
5. Read `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/Contracts/I{ModuleName}Module.cs` — note `ExecuteCommandAsync` / `ExecuteQueryAsync` signatures.
6. If an existing `{Aggregate}Tests.cs` exists in IntegrationTests — read it and determine:
   - Constructor style (shared setup vs. empty constructor)?
   - Test method signature (`async void` or `async Task`)?
   - SampleData style: inline `private static readonly` constants vs. separate `{Aggregate}SampleData` struct?
   - Which tests already exist? (No duplicates.)
7. If `{Aggregate}SampleData.cs` exists in IntegrationTests — read it: note all constants, GUIDs, and DateTime style (`SystemClock.Now` vs. `new DateTime(...)`).
8. If `{Aggregate}Factory.cs` exists in IntegrationTests — read it: note which command it dispatches and what `SampleData` it references.
9. If existing probe files exist for this aggregate — read them: note whether they use the optional `satisfiedWhen` predicate parameter (LabAnalysis style) or fixed `IsSatisfied` (TestOrders style).
10. If the requirement involves reacting to an external event: list `src/HC.LIS/HC.LIS.Modules/{SourceModule}/IntegrationEvents/` — identify the relevant `IntegrationEvent` type and its constructor parameters.

---

## Phase 3 — Derive names and classify tests

### Name derivation

Show derived names to the user before generating. Ask for confirmation if any derivation is uncertain.

| Artifact | Naming rule | Example |
|---|---|---|
| Command | `{Action}{Entity}Command` | `ArchiveWorklistItemCommand` |
| Query | `Get{Entity}DetailsQuery` | `GetWorklistItemDetailsQuery` |
| DTO | `{Entity}DetailsDto` | `WorklistItemDetailsDto` |
| Probe (with predicate) | `Get{Entity}DetailsFrom{ModuleName}Probe` | `GetWorklistItemDetailsFromLabAnalysisProbe` |
| Probe (fixed) | `Get{Entity}{State}From{ModuleName}Probe` | `GetOrderItemInProgressFromTestOrdersProbe` |
| Test class | `{Entity}Tests` | `WorklistItemTests` |
| Happy-path test method | `{Action}{Entity}IsSuccessful` | `ArchiveWorklistItemIsSuccessful` |
| Multi-step test method | `{Action}{Entity}IsSuccessful` | `GenerateReportIsSuccessful` |
| Inbox injection test method | `{Action}{Entity}Via{EventName}IsSuccessful` | `PlaceExamInProgressViaSampleCollectedIsSuccessful` |
| Factory | `{Aggregate}Factory` | `WorklistItemFactory` |
| SampleData | `{Aggregate}SampleData` | `WorklistItemSampleData` |

**CA1707:** PascalCase only — no underscores in any test method name.

### Test classification

| Test type | Generate when |
|---|---|
| Command test (happy path) | Requirement describes a single new command dispatched through the module |
| Multi-step test | Requirement requires prior state established by preceding commands before the act step |
| Inbox injection test | Requirement describes behavior triggered by an integration event from another module via InboxMessages |

---

## Phase 4 — Clarify if needed

Check for ambiguity before writing:
- Which aggregate/entity owns the behavior?
- Does the module facade expose a `Get{Aggregate}Details` query already, or will it be new?
- What is the expected final `Status` value after the action?
- Does the test need a probe with the optional predicate (`satisfiedWhen`) or a separate named probe per status?
- Does `SampleData` need new GUIDs or scalar constants?
- If inbox injection: which source module and `IntegrationEvent` type? What SQL schema is used for InboxMessages?
- Does `{Aggregate}Factory.cs` need to be created or does it already exist?

Ask at most one focused question. Do not generate speculative code.

---

## Phase 5 — Generate test files

Write only test files. Do not touch domain, application, or infrastructure code.

### 5a. Decision matrix — what to write or edit

| Condition | Action |
|---|---|
| `{Aggregate}Tests.cs` exists in IntegrationTests | Edit — append new `[Fact]` methods inside existing class |
| `{Aggregate}Tests.cs` absent | Create — full class with empty constructor |
| Separate `{Aggregate}SampleData.cs` used by module | Edit — append new `static readonly` GUIDs and `const string` scalars |
| Inline constants in test class | Add new `private static readonly` fields inside the test class |
| `{Aggregate}Factory.cs` absent in IntegrationTests | Create — single `CreateAsync()` dispatching the root create command |
| `{Aggregate}Factory.cs` exists and new precondition needed | Edit — append `CreateWith{State}Async()` chaining prior state |
| Probe for this aggregate/DTO absent | Create — new probe class (see template choice below) |
| Probe with predicate already exists for this DTO | Reuse existing probe with a `satisfiedWhen` lambda |

**Probe template choice:**
- **Variant A (with predicate):** when the same DTO will be polled at multiple states in the same test file (LabAnalysis style).
- **Variant B (fixed):** when the probe targets one specific final state (TestOrders style).
- **Variant C (DB-direct):** when the `Get{Aggregate}DetailsQuery` does not yet exist — probe queries the read-model table directly via Dapper so the test file compiles.

**Test constructor style:**
- Match existing `{Aggregate}Tests.cs` if it exists.
- When creating new: default to empty constructor `public {Aggregate}Tests() : base(Guid.CreateVersion7()) { }` with all setup inside each `[Fact]` method.

**Test method async style:**
- Match existing `{Aggregate}Tests.cs` if it exists.
- When creating new: default to `async Task`.

Namespace:
```csharp
namespace HC.LIS.Modules.{ModuleName}.IntegrationTests.{Aggregate}s;
```

### 5b. Templates

See `.claude/skills/integration-test/templates/command-test.md`
See `.claude/skills/integration-test/templates/multi-step-test.md`
See `.claude/skills/integration-test/templates/inbox-injection-test.md`
See `.claude/skills/integration-test/templates/probe.md`

---

## Phase 6 — Verify (expected to fail)

```bash
dotnet build src/HC.LIS/HC.LIS.Modules/{ModuleName}/Tests/IntegrationTests/
```

**Acceptable:** "type or namespace not found", "does not contain a definition for" for new command/query/DTO types.
**Fix before reporting:** syntax errors, namespace mismatches, duplicate members, missing `using` directives for types that already exist.

Always output at the end:

> These tests are intentionally failing — `{CommandType}`, `{QueryType}`, and `{DtoType}` do not exist yet. This is the TDD failing test.
>
> Next step: run `/unit-test {ModuleName} {requirement}` to write the domain-layer unit tests.

---

## Analyzer rules (TreatWarningsAsErrors=true)

| Rule | What to do |
|---|---|
| CA1707 | PascalCase only in test method names |
| CA2007 | `.ConfigureAwait(true)` on every `await` in test methods; `.ConfigureAwait(false)` in probe `GetSampleAsync()` and factory `CreateAsync()` |

## Code style

- File-scoped namespaces
- 4-space indentation
- `using` blocks (`using (var connection = ...)`) around `NpgsqlConnection` in inbox injection tests
- Explicit type for DTO results: `{DtoType}? details = await GetEventually(...)` — not `var`
- `var` for local intermediate objects: `var integrationEvent = new ...`
- `Guid.CreateVersion7()` for all new GUIDs
- `const string` for string constants in SampleData; `static readonly Guid` for GUIDs
- Use `SystemClock.Now` for timestamps if the module's existing SampleData uses it; otherwise `new DateTime(...)` literals
- All `await` in test methods end with `.ConfigureAwait(true)`
- All `await` in probe `GetSampleAsync()` and factory methods end with `.ConfigureAwait(false)`

---

## Critical reference files

| File | What it shows |
|---|---|
| `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/OrderTests.cs` | Shared-constructor style, `async void`, separate SampleData struct |
| `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/WorklistItems/WorklistItemTests.cs` | Self-contained style, `async Task`, inline constants, multi-step and inbox injection |
| `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/PlaceExamInProgressViaSampleCollectedTests.cs` | Inbox injection pattern with shared-constructor setup |
| `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/WorklistItems/GetWorklistItemDetailsFromLabAnalysisProbe.cs` | Probe with optional predicate parameter (Variant A) |
| `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/GetOrderItemInProgressFromTestOrdersProbe.cs` | Fixed `IsSatisfied` probe (Variant B) |
| `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/OrderFactory.cs` | Integration test factory (async, dispatches via module) |
| `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Tests/IntegrationTests/TestBase.cs` | Module facade property name, SQL schema string, startup class |
