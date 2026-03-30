# LabAnalysis Module — Implementation Tasks

**Status:** Implementation complete — code review done 2026-03-28, bugs and design issues logged below
**Spec reference:** [docs/specs/LabAnalysis-TechSpec.md](../specs/LabAnalysis-TechSpec.md)
**PRD reference:** [docs/prd/LabAnalysis.md](../prd/LabAnalysis.md)

> **Convention:** After finishing each step, update this file — check off completed items and add a session note with any implementation details worth remembering.

---

## Prerequisites

- [x] Extend `SampleCollectedIntegrationEvent` (in SampleCollection module) with three new fields:
  - `SampleBarcode (string)`
  - `PatientId (Guid)`
  - `ExamCodes (IReadOnlyCollection<string>)` — one entry per exam ordered on this sample; **not** a single string, because the same collected sample may carry multiple ordered exams, each producing a distinct analyte
  - File: `src/HC.LIS/HC.LIS.Modules/SampleCollection/IntegrationEvents/SampleCollectedIntegrationEvent.cs`
  - Also update `RecordSampleCollection/SampleCollectedPublishEventNotificationHandler.cs` to populate the new fields from `notification.DomainEvent`
  - Verify unit + integration tests in SampleCollection still pass

> **Session note:** `ExamCodes` is mapped from `ExamIds (IReadOnlyCollection<Guid>)` on the domain event using `.Select(id => id.ToString())`. The domain only has GUIDs; string exam codes are a LabAnalysis-layer concept. TestOrders `SampleCollectedIntegrationEventHandler` was updated to `Guid.Parse` each code back. Old `ExamIds` property was removed from integration event.

---

## Layer 0: Domain

One subfolder per concern under `Domain/`.

### Aggregate

- [x] `Domain/WorklistItems/WorklistItemId.cs` — typed ID wrapping `Guid`
- [x] `Domain/WorklistItems/WorklistItem.cs` — aggregate root
  - `Create(Guid worklistItemId, Guid sampleId, string sampleBarcode, string examCode, Guid patientId, DateTime createdAt)` → `WorklistItemCreatedDomainEvent`
  - `RecordResult(string resultValue, Guid analystId, DateTime recordedAt)` → checks `CannotRecordResultForNonPendingWorklistItemRule` → `AnalysisResultRecordedDomainEvent`
  - `GenerateReport(string reportPath, DateTime generatedAt)` → checks `CannotGenerateReportWithoutResultRule` → `ReportGeneratedDomainEvent`
  - `Complete(string completionType, DateTime completedAt)` → checks `CannotCompleteWorklistItemWithoutReportRule` → `WorklistItemCompletedDomainEvent`
  - Private `Apply()` methods for each event (rebuilds status from event stream)

### Domain Events

- [x] `Domain/WorklistItems/Events/WorklistItemCreatedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `SampleId (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`, `PatientId (Guid)`, `CreatedAt (DateTime)`
- [x] `Domain/WorklistItems/Events/AnalysisResultRecordedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `ResultValue (string)`, `AnalystId (Guid)`, `RecordedAt (DateTime)`
- [x] `Domain/WorklistItems/Events/ReportGeneratedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `ReportPath (string)`, `GeneratedAt (DateTime)`
- [x] `Domain/WorklistItems/Events/WorklistItemCompletedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `SampleId (Guid)`, `ExamCode (string)`, `CompletionType (string)`, `CompletedAt (DateTime)`

### Business Rules

- [x] `Domain/WorklistItems/Rules/CannotRecordResultForNonPendingWorklistItemRule.cs`
  - Broken when status != `"Pending"`
- [x] `Domain/WorklistItems/Rules/CannotGenerateReportWithoutResultRule.cs`
  - Broken when status != `"ResultReceived"`
- [x] `Domain/WorklistItems/Rules/CannotCompleteWorklistItemWithoutReportRule.cs`
  - Broken when status != `"ReportGenerated"`

---

## Layer 1: Unit Tests

Write tests before or alongside domain implementation (TDD).

- [x] `Tests/UnitTests/WorklistItems/WorklistItemTests.cs`

| Test Method | Validates |
|---|---|
| `CreateWorklistItemIsSuccessful` | `WorklistItemCreatedDomainEvent` raised with correct fields |
| `RecordAnalysisResultIsSuccessful` | `AnalysisResultRecordedDomainEvent` raised |
| `GenerateReportIsSuccessful` | `ReportGeneratedDomainEvent` raised |
| `CompleteWorklistItemIsSuccessful` | `WorklistItemCompletedDomainEvent` raised |
| `RecordResultThrowsWhenNotPending` | `BaseBusinessRuleException` for `CannotRecordResultForNonPendingWorklistItemRule` |
| `GenerateReportThrowsWhenResultNotReceived` | `BaseBusinessRuleException` for `CannotGenerateReportWithoutResultRule` |
| `CompleteThrowsWhenReportNotGenerated` | `BaseBusinessRuleException` for `CannotCompleteWorklistItemWithoutReportRule` |

Supporting test infrastructure:
- [x] `Tests/UnitTests/WorklistItems/WorklistItemFactory.cs` — builds `WorklistItem` aggregates at various states for test setup

> **Session note:** `WorklistItem` extends `AggregateRoot` (not `Entity`), so `TestBase` needed `AggregateRoot` overloads for `AssertPublishedDomainEvent<T>`. `WorklistItemSampleData` fields must be `const string`, not `static readonly string` (CA1802).

---

## Layer 2: Application — Commands & Handlers

One subfolder per command under `Application/WorklistItems/`.

### Step 1 — CreateWorklistItem

- [x] `Application/WorklistItems/CreateWorklistItem/CreateWorklistItemCommand.cs`
  - Properties: `WorklistItemId`, `SampleId`, `PatientId` (all `Guid`), `SampleBarcode`, `ExamCode` (both `string`), `CreatedAt` (`DateTime`)
  - Extends `CommandBase<Guid>`
- [x] `Application/WorklistItems/CreateWorklistItem/CreateWorklistItemCommandHandler.cs`
  - Calls `WorklistItem.Create(...)`, persists via `IAggregateStore.Start()`
  - Returns `command.WorklistItemId`

### Step 2 — RecordAnalysisResult

- [x] `Application/WorklistItems/RecordAnalysisResult/RecordAnalysisResultCommand.cs`
  - Properties: `WorklistItemId (Guid)`, `ResultValue (string)`, `AnalystId (Guid)`, `RecordedAt (DateTime)`
  - Extends `CommandBase`
- [x] `Application/WorklistItems/RecordAnalysisResult/RecordAnalysisResultCommandHandler.cs`
  - Loads via `new WorklistItemId(command.WorklistItemId)`, calls `RecordResult(...)`, saves via `AppendChanges`

### Step 3 — GenerateReport

- [x] `Application/WorklistItems/GenerateReport/GenerateReportCommand.cs`
  - Properties: `WorklistItemId (Guid)`, `ReportPath (string)`, `GeneratedAt (DateTime)`
  - Extends `CommandBase`
- [x] `Application/WorklistItems/GenerateReport/GenerateReportCommandHandler.cs`
  - Loads, calls `GenerateReport(...)`, saves

### Step 4 — CompleteWorklistItem

- [x] `Application/WorklistItems/CompleteWorklistItem/CompleteWorklistItemCommand.cs`
  - Properties: `WorklistItemId (Guid)`, `CompletionType (string)`, `CompletedAt (DateTime)`
  - Extends `CommandBase`
- [x] `Application/WorklistItems/CompleteWorklistItem/CompleteWorklistItemCommandHandler.cs`
  - Loads, calls `Complete(...)`, saves

---

## Layer 3: Application — Notifications

### Notification files

- [x] `Application/WorklistItems/CreateWorklistItem/WorklistItemCreatedNotification.cs`
- [x] `Application/WorklistItems/CreateWorklistItem/WorklistItemCreatedPublishEventNotificationHandler.cs`
  — publishes `WorklistItemCreatedIntegrationEvent` via `IEventsBus`
- [x] `Application/WorklistItems/RecordAnalysisResult/AnalysisResultRecordedNotification.cs`
- [x] `Application/WorklistItems/GenerateReport/ReportGeneratedNotification.cs`
- [x] `Application/WorklistItems/CompleteWorklistItem/WorklistItemCompletedNotification.cs`
- [x] `Application/WorklistItems/CompleteWorklistItem/WorklistItemCompletedPublishEventNotificationHandler.cs`
  — publishes `WorklistItemCompletedIntegrationEvent` via `IEventsBus`

---

## Layer 4: Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable, JSON-serializable.

- [x] `IntegrationEvents/WorklistItemCreatedIntegrationEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `PatientId (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`
- [x] `IntegrationEvents/WorklistItemCompletedIntegrationEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `SampleId (Guid)`, `ExamCode (string)`, `CompletionType (string)`

---

## Layer 5: Infrastructure Wiring

### DomainEventTypeMappings

- [x] `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`
  - Register all 4 domain events (both via `Dictionary` and `MartenConfig.cs` `AddEventType<>`)

```csharp
// DomainEventTypeMappings Dictionary:
{ "WorklistItemCreatedDomainEvent",      typeof(WorklistItemCreatedDomainEvent) },
{ "AnalysisResultRecordedDomainEvent",   typeof(AnalysisResultRecordedDomainEvent) },
{ "ReportGeneratedDomainEvent",          typeof(ReportGeneratedDomainEvent) },
{ "WorklistItemCompletedDomainEvent",    typeof(WorklistItemCompletedDomainEvent) },

// MartenConfig.cs:
options.Events.AddEventType<WorklistItemCreatedDomainEvent>();
options.Events.AddEventType<AnalysisResultRecordedDomainEvent>();
options.Events.AddEventType<ReportGeneratedDomainEvent>();
options.Events.AddEventType<WorklistItemCompletedDomainEvent>();
```

### LabAnalysisStartup — OutboxModule BiMap

- [x] `Infrastructure/Configurations/LabAnalysisStartup.cs`
  - Populate BiMap with all 4 notification mappings (4/4):

```csharp
notificationsBiMap.Add("WorklistItemCreatedNotification",      typeof(WorklistItemCreatedNotification));
notificationsBiMap.Add("AnalysisResultRecordedNotification",   typeof(AnalysisResultRecordedNotification));
notificationsBiMap.Add("ReportGeneratedNotification",          typeof(ReportGeneratedNotification));
notificationsBiMap.Add("WorklistItemCompletedNotification",    typeof(WorklistItemCompletedNotification));
```

### EventsBus Subscription

- [x] `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs`
  - Register subscription to `SampleCollectedIntegrationEvent` → `SampleCollectedIntegrationEventNotificationHandler`
- [x] `Infrastructure/Configurations/EventsBus/SampleCollectedIntegrationEventNotificationHandler.cs`
  - Implements `INotificationHandler<SampleCollectedIntegrationEvent>`
  - Iterates `integrationEvent.ExamCodes` — dispatches **one `CreateWorklistItemCommand` per exam code**, each with its own `Guid.CreateVersion7()` as `WorklistItemId`

> **Session note:** Handler must end in `NotificationHandler`, not `EventHandler` (CA1711). The Application project needs a `<ProjectReference>` to `SampleCollection.IntegrationEvents.csproj`.

### Module Facade

- [x] `ILabAnalysisModule.cs` — already scaffolded, no changes needed
- [x] `LabAnalysisModule.cs` — already scaffolded, no changes needed

---

## Layer 6: Read Model — WorklistItemDetails

### Database Migration

- [x] `src/HC.LIS/HC.LIS.Database/LabAnalysis/20260326120400_LabAnalysisModule_AddTableWorklistItemDetails.cs`

```sql
-- Schema: lab_analysis
CREATE TABLE lab_analysis.worklist_item_details (
    id UUID PRIMARY KEY,
    sample_id UUID NOT NULL,
    sample_barcode VARCHAR(255) NOT NULL,
    exam_code VARCHAR(255) NOT NULL,
    patient_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    result_value TEXT NULL,
    report_path VARCHAR(500) NULL,
    completion_type VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ NULL
);
```

### Application Files

- [x] `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsDto.cs`
  - All columns above as public properties
- [x] `Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQuery.cs`
  - Property: `WorklistItemId (Guid)`, extends `QueryBase<WorklistItemDetailsDto>`
- [x] `Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQueryHandler.cs`
  - Dapper SELECT from `lab_analysis.worklist_item_details WHERE id = @worklistItemId`
- [x] `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsProjector.cs`
  - `When(WorklistItemCreatedDomainEvent)` → INSERT, Status = `"Pending"`
  - `When(AnalysisResultRecordedDomainEvent)` → UPDATE ResultValue, Status = `"ResultReceived"`
  - `When(ReportGeneratedDomainEvent)` → UPDATE ReportPath, Status = `"ReportGenerated"`
  - `When(WorklistItemCompletedDomainEvent)` → UPDATE Status = `"Completed"`, CompletionType, CompletedAt
  - `When(IDomainEvent)` → no-op

### Notification Projection Handlers (co-located with commands)

- [x] `Application/WorklistItems/CreateWorklistItem/WorklistItemCreatedNotificationProjection.cs`
- [x] `Application/WorklistItems/RecordAnalysisResult/AnalysisResultRecordedNotificationProjection.cs`
- [x] `Application/WorklistItems/GenerateReport/ReportGeneratedNotificationProjection.cs`
- [x] `Application/WorklistItems/CompleteWorklistItem/WorklistItemCompletedNotificationProjection.cs`

---

## Layer 7: Integration Tests

Location: `Tests/IntegrationTests/WorklistItems/`
Pattern: command → `GetEventually(probe, 15000)` on `WorklistItemDetails` read model.

### Test Infrastructure

- [x] `Tests/IntegrationTests/WorklistItems/GetWorklistItemDetailsFromLabAnalysisProbe.cs` — `IProbe<WorklistItemDetailsDto>`
- [x] `Tests/IntegrationTests/TestBase.cs`
  - `ClearDatabase()` truncates `lab_analysis.worklist_item_details`
  - Add `GetEventually()` static method (identical pattern to SampleCollection/TestOrders)
  - `IDisposable` calls `LabAnalysisStartup.Stop()`

### Test Methods

- [x] `Tests/IntegrationTests/WorklistItems/WorklistItemTests.cs`

| Test Method | Command Sent | Assertion |
|---|---|---|
| `CreateWorklistItemIsSuccessful` | `CreateWorklistItemCommand` | `WorklistItemDetails.Status == "Pending"`, `SampleBarcode`, `ExamCode`, `PatientId` set |
| `RecordAnalysisResultIsSuccessful` | `RecordAnalysisResultCommand` | `Status == "ResultReceived"`, `ResultValue` set |
| `GenerateReportIsSuccessful` | `GenerateReportCommand` | `Status == "ReportGenerated"`, `ReportPath` set |
| `CompleteWorklistItemIsSuccessful` | `CompleteWorklistItemCommand` | `Status == "Completed"`, `CompletionType` set, `CompletedAt` set |

---

## Code Review Findings (2026-03-28)

Items surfaced during post-implementation code review against the PRD and clinical workflow analysis.

---

### Bug Fix 1 — `CreateWorklistItemCommand` not registered in `internalCommandsMap` (critical)

`LabAnalysisStartup.cs` initializes an empty `internalCommandsMap`. The Quartz job that processes internal commands uses this map to deserialize queued command types from JSON. Since `CreateWorklistItemCommand` is not registered, it is silently dropped after being enqueued — the entire `SampleCollected` → `CreateWorklistItem` runtime flow is broken. Integration tests don't catch this because they call `ExecuteCommandAsync` directly.

- [x] `Infrastructure/Configurations/LabAnalysisStartup.cs`
  - Add to `internalCommandsMap`:
    ```csharp
    internalCommandsMap.Add("CreateWorklistItemCommand", typeof(CreateWorklistItemCommand));
    ```
  - Reference: `TestOrdersStartup.cs:75` shows the same pattern

---

### Bug Fix 2 — `HC.COre` namespace typo in integration events

Namespace casing bug: `HC.COre` (capital O) instead of `HC.Core`. Works on Windows (case-insensitive FS) but breaks on Linux CI.

- [x] `IntegrationEvents/WorklistItemCreatedIntegrationEvent.cs:2` — fix `HC.COre` → `HC.Core`
- [x] `IntegrationEvents/WorklistItemCompletedIntegrationEvent.cs:2` — fix `HC.COre` → `HC.Core`
- [x] `SampleCollection/IntegrationEvents/SampleCollectedIntegrationEvent.cs:2` — same typo (predates this module)
- [x] `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs:4` — same typo (`using HC.COre.Infrastructure.EventBus`)

> **Session note (2026-03-29):** The typo originated in `HC.Core/Infrastructure/EventBus/IntegrationEvent.cs` — the class itself declared `namespace HC.COre.Infrastructure.EventBus`. All consumers were correct (matching the declared namespace). Fix was to rename the namespace in the base class and do a project-wide `HC.COre` → `HC.Core` replacement (15 files total). Some files had duplicate usings after the replacement and were cleaned up.

---

### Feature — Structured Result Value

`resultValue` is currently a plain `string` (e.g., `"7.4 mmol/L"`). Clinical results require three separate fields to be meaningful for report generation, reference range evaluation, and downstream consumers.

New fields: `ResultValue (string)`, `ResultUnit (string)`, `ReferenceRange (string)`.

#### Domain

- [x] `Domain/WorklistItems/Events/AnalysisResultRecordedDomainEvent.cs`
  - Replace `ResultValue (string)` with three properties: `ResultValue (string)`, `ResultUnit (string)`, `ReferenceRange (string)`
- [x] `Domain/WorklistItems/WorklistItem.cs`
  - Update `RecordResult(string resultValue, string resultUnit, string referenceRange, Guid performedById, DateTime recordedAt)`

#### Unit Tests

- [x] `Tests/UnitTests/WorklistItems/WorklistItemTests.cs`
  - Update `RecordAnalysisResultIsSuccessful` to assert all three result fields on the domain event
- [x] `Tests/UnitTests/WorklistItems/WorklistItemFactory.cs`
  - Update `CreateWithResult()` helper with the new signature

#### Application

- [x] `Application/WorklistItems/RecordAnalysisResult/RecordAnalysisResultCommand.cs`
  - Add `ResultUnit (string)` and `ReferenceRange (string)` properties
- [x] `Application/WorklistItems/RecordAnalysisResult/RecordAnalysisResultCommandHandler.cs`
  - Pass new fields to `RecordResult(...)`

#### Database Migration

- [x] `src/HC.LIS/HC.LIS.Database/LabAnalysis/20260326120400_LabAnalysisModule_AddTableWorklistItemDetails.cs`
  - Added `result_unit VARCHAR(50) NULL` and `reference_range VARCHAR(100) NULL` columns directly to `CREATE TABLE` (migration not yet deployed)

#### Read Model

- [x] `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsDto.cs`
  - Add `ResultUnit (string?)` and `ReferenceRange (string?)` properties
- [x] `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsProjector.cs`
  - Update `When(AnalysisResultRecordedDomainEvent)` to also SET `result_unit`, `reference_range`

#### Integration Tests

- [x] `Tests/IntegrationTests/WorklistItems/WorklistItemTests.cs`
  - Update `RecordAnalysisResultIsSuccessful` to assert `ResultUnit` and `ReferenceRange` on the DTO

---

### Feature — Inbound Analyzer Result Integration Event

FR3 requires results to come from clinical analyzers via events (not a manually-triggered command). Define the inbound event contract and wire it into LabAnalysis so the analyzer integration layer has a defined interface to publish to.

> The `AnalystId` field on `RecordResult` is retained for future use (analyst validation step). For now, it is set from the integration event payload (instrument ID or a system sentinel value).

#### Integration Event Contract

- [x] `IntegrationEvents/AnalyzerResultReceivedIntegrationEvent.cs`
  - Owned by LabAnalysis module
  - Properties: `WorklistItemId (Guid)`, `InstrumentId (Guid)`, `ResultValue (string)`, `ResultUnit (string)`, `ReferenceRange (string)`, `RecordedAt (DateTime)`

#### EventsBus Subscription

- [x] `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs`
  - Register subscription to `AnalyzerResultReceivedIntegrationEvent`
- [x] `Application/WorklistItems/RecordAnalysisResult/AnalyzerResultReceivedIntegrationEventNotificationHandler.cs`
  - Implements `INotificationHandler<AnalyzerResultReceivedIntegrationEvent>`
  - Enqueues `RecordAnalysisResultCommand` via `ICommandsScheduler`
  - Maps `InstrumentId` → `PerformedById`

#### Infrastructure Wiring

- [x] `Infrastructure/Configurations/LabAnalysisStartup.cs`
  - Add `RecordAnalysisResultCommand` to `internalCommandsMap`

#### Integration Test

- [x] `Tests/IntegrationTests/WorklistItems/WorklistItemTests.cs`
  - Added `RecordAnalysisResultViaAnalyzerEventIsSuccessful`
  - Inserts `AnalyzerResultReceivedIntegrationEvent` directly into `lab_analysis.InboxMessages`
  - Polls read model for `Status == "ResultReceived"` and asserts `ResultValue`, `ResultUnit`, `ReferenceRange`

---

### Design Issue — No idempotency guard on `SampleCollected` → `CreateWorklistItem`

`SampleCollectedIntegrationEventNotificationHandler` generates a fresh `Guid.CreateVersion7()` for each `WorklistItemId` on every invocation. Under at-least-once inbox delivery the same message could be processed twice, silently creating duplicate `WorklistItem` streams for the same `(sampleId, examCode)` pair. Marten would not reject these since each gets a unique stream ID.

- [x] Replaced `Guid.CreateVersion7()` with a deterministic ID derived from `(sampleId, examCode)` using SHA256 (v5-style UUID)
  - File: `Application/WorklistItems/HandleSampleCollected/SampleCollectedIntegrationEventNotificationHandler.cs`

---

### Design Issue — `CompletionType` unconstrained at the domain level

`Complete(string completionType, ...)` accepts any string. The TechSpec states `WorklistItem` always completes as `"Complete"` (PartialComplete is TestOrders' responsibility). Nothing in the aggregate enforces this — a caller can pass `"PartialComplete"` and it will silently be stored, contradicting the spec.

- [x] Removed `completionType` parameter from `Complete()` — hardcoded `"Complete"` in the domain event per TechSpec
  - Files: `Domain/WorklistItems/WorklistItem.cs`, `CompleteWorklistItemCommand.cs`, `CompleteWorklistItemCommandHandler.cs`

---

### Design Issue — `GenerateReport` must be automated as an internal command

In a clinical lab, report generation fires automatically after a result is validated — it is never a separate manual external call. Currently nothing triggers `GenerateReportCommand` after `RecordAnalysisResult`; the caller is expected to fire it separately.

- [x] Promote `GenerateReportCommand` to an internal command (`InternalCommandBase`) and have `AnalysisResultRecordedNotification` enqueue it automatically
  - New handler: `Application/WorklistItems/RecordAnalysisResult/AnalysisResultRecordedScheduleReportNotificationHandler.cs`
  - Report path derived as `/reports/worklist/{worklistItemId}.pdf`
  - Register in `internalCommandsMap`:
    ```csharp
    internalCommandsMap.Add("GenerateReportCommand", typeof(GenerateReportCommand));
    ```
  - File: `Infrastructure/Configurations/LabAnalysisStartup.cs`

---

### Design Issue — `AnalystId` semantically maps to an instrument

`RecordResult` takes `analystId (Guid)`. Per the TechSpec, when the analyzer event integration is built, `InstrumentId` from the event is mapped to this field. An instrument is not an analyst; the naming will mislead future readers.

- [x] Rename `AnalystId` → `PerformedById` across domain event, aggregate, command, and handler — or explicitly split into `AnalystId` (human) and `InstrumentId` (machine) once the analyzer event is wired
  - Files: `Domain/WorklistItems/Events/AnalysisResultRecordedDomainEvent.cs`, `WorklistItem.cs:42`, `RecordAnalysisResultCommand.cs`, `RecordAnalysisResultCommandHandler.cs`

---

### Design Issue — No `Reject()` path on the aggregate

In clinical labs, samples are rejected before analysis (haemolysis, insufficient volume, wrong tube type, sample mix-up). Currently the state machine has no rejection transition — rejected items would remain `Pending` indefinitely with no way to close them out.

> Out of scope for v1 — document in PRD open questions.

- [x] Added open question #6 to `docs/prd/LabAnalysis.md` (sample rejection path)
- [ ] (future) Add `Reject(string rejectionReason, Guid rejectedById, DateTime rejectedAt)` method and `WorklistItemRejectedDomainEvent` to the aggregate
  - State machine: `Pending → Rejected` (terminal)

---

### Design Issue — No pathologist review / sign-off step

The PRD lists Pathologist as a primary persona, but the current state machine jumps `ReportGenerated → Completed` with no validation gate. In practice, out-of-range or flagged results are held for pathologist review before the report is released.

> Out of scope for v1 — document in PRD open questions.

- [x] Added open question #7 to `docs/prd/LabAnalysis.md` (pathologist sign-off)
- [ ] (future) Add `Validate(Guid pathologistId, DateTime validatedAt)` method and `PendingValidation` state to the aggregate

---

### Design Issue — `DateTime` vs `DateTimeOffset` for clinical timestamps

Commands and events use `DateTime` throughout. Clinical lab systems require timezone-correct timestamps for HIPAA-compliant audit trails (CAP/CLIA). `DateTime` without explicit `DateTimeKind.Utc` can produce ambiguous records.

- [x] Verified: `SystemClock.Now` returns `DateTime.UtcNow` — annotated with UTC convention comment in `HC.Core/Domain/SystemClock.cs`

---

## Verification Checklist

- [x] `dotnet build` — zero warnings (`TreatWarningsAsErrors=true`)
- [x] `dotnet test` UnitTests — 7/7 passed
- [x] `dotnet test` IntegrationTests — 4/4 passed
- [x] SampleCollection `dotnet test` UnitTests — 14/14 passed (after prerequisite change)
- [x] SampleCollection `dotnet test` IntegrationTests — 7/7 passed (after prerequisite change)

> **Session note (2026-03-27):** Migration runner uses `ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING`; integration tests use `ASPNETCORE_HCLIS_IntegrationTests_ConnectionString`. Must set the former explicitly when running migrations (the default shell env doesn't have it set). Run: `ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=Healthcore.Dev;Username=dev;Password=dev" dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj`

### Analyzer Rules to Watch

- `CA1002`: expose `IReadOnlyCollection<T>`, not `List<T>`
- `CA2007`: `.ConfigureAwait(false)` on awaited tasks (`.ConfigureAwait(true)` inside `[Fact]` methods)
- `CA1707`: PascalCase test method names (no underscores)
- `CA1716`: avoid reserved keywords in namespaces

---

## Reference Files

| Purpose | Path |
|---|---|
| Command + Handler pattern | `src/HC.LIS/HC.LIS.Modules/SampleCollection/Application/Collections/*/` |
| Notification + publish handler | `src/HC.LIS/HC.LIS.Modules/SampleCollection/Application/Collections/*/` |
| Integration event pattern | `src/HC.LIS/HC.LIS.Modules/SampleCollection/IntegrationEvents/` |
| EventsBus subscription handler | `src/HC.LIS/HC.LIS.Modules/TestOrders/Infrastructure/Configurations/EventsBus/` |
| Startup / BiMap wiring | `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/Configurations/LabAnalysisStartup.cs` |
| DomainEventTypeMappings | `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs` |
| MartenConfig | `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/Configurations/DataAccess/MartenConfig.cs` |
| Module facade | `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/LabAnalysisModule.cs` |
| Projection pattern | `src/HC.LIS/HC.LIS.Modules/SampleCollection/Application/Collections/GetCollectionRequestDetails/` |
| Integration test pattern | `src/HC.LIS/HC.LIS.Modules/SampleCollection/Tests/IntegrationTests/` |
| **Target module root** | `src/HC.LIS/HC.LIS.Modules/LabAnalysis/` |
| **Target DB migrations** | `src/HC.LIS/HC.LIS.Database/LabAnalysis/` |
