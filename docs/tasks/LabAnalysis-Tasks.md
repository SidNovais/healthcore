# LabAnalysis Module — Implementation Tasks

**Status:** Not started
**Spec reference:** [docs/specs/LabAnalysis-TechSpec.md](../specs/LabAnalysis-TechSpec.md)
**PRD reference:** [docs/prd/LabAnalysis.md](../prd/LabAnalysis.md)

> **Convention:** After finishing each step, update this file — check off completed items and add a session note with any implementation details worth remembering.

---

## Prerequisites

- [ ] Extend `SampleCollectedIntegrationEvent` (in SampleCollection module) with three new fields:
  - `SampleBarcode (string)`
  - `PatientId (Guid)`
  - `ExamCodes (IReadOnlyCollection<string>)` — one entry per exam ordered on this sample; **not** a single string, because the same collected sample may carry multiple ordered exams, each producing a distinct analyte
  - File: `src/HC.LIS/HC.LIS.Modules/SampleCollection/IntegrationEvents/SampleCollectedIntegrationEvent.cs`
  - Also update `RecordSampleCollection/SampleCollectedPublishEventNotificationHandler.cs` to populate the new fields from `notification.DomainEvent`
  - Verify unit + integration tests in SampleCollection still pass

---

## Layer 0: Domain

One subfolder per concern under `Domain/`.

### Aggregate

- [ ] `Domain/WorklistItems/WorklistItemId.cs` — typed ID wrapping `Guid`
- [ ] `Domain/WorklistItems/WorklistItem.cs` — aggregate root
  - `Create(Guid worklistItemId, Guid sampleId, string sampleBarcode, string examCode, Guid patientId, DateTime createdAt)` → `WorklistItemCreatedDomainEvent`
  - `RecordResult(string resultValue, Guid analystId, DateTime recordedAt)` → checks `CannotRecordResultForNonPendingWorklistItemRule` → `AnalysisResultRecordedDomainEvent`
  - `GenerateReport(string reportPath, DateTime generatedAt)` → checks `CannotGenerateReportWithoutResultRule` → `ReportGeneratedDomainEvent`
  - `Complete(string completionType, DateTime completedAt)` → checks `CannotCompleteWorklistItemWithoutReportRule` → `WorklistItemCompletedDomainEvent`
  - Private `Apply()` methods for each event (rebuilds status from event stream)

### Domain Events

- [ ] `Domain/WorklistItems/Events/WorklistItemCreatedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `SampleId (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`, `PatientId (Guid)`, `CreatedAt (DateTime)`
- [ ] `Domain/WorklistItems/Events/AnalysisResultRecordedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `ResultValue (string)`, `AnalystId (Guid)`, `RecordedAt (DateTime)`
- [ ] `Domain/WorklistItems/Events/ReportGeneratedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `ReportPath (string)`, `GeneratedAt (DateTime)`
- [ ] `Domain/WorklistItems/Events/WorklistItemCompletedDomainEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `SampleId (Guid)`, `ExamCode (string)`, `CompletionType (string)`, `CompletedAt (DateTime)`

### Business Rules

- [ ] `Domain/WorklistItems/Rules/CannotRecordResultForNonPendingWorklistItemRule.cs`
  - Broken when status != `"Pending"`
- [ ] `Domain/WorklistItems/Rules/CannotGenerateReportWithoutResultRule.cs`
  - Broken when status != `"ResultReceived"`
- [ ] `Domain/WorklistItems/Rules/CannotCompleteWorklistItemWithoutReportRule.cs`
  - Broken when status != `"ReportGenerated"`

---

## Layer 1: Unit Tests

Write tests before or alongside domain implementation (TDD).

- [ ] `Tests/UnitTests/WorklistItems/WorklistItemTests.cs`

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
- [ ] `Tests/UnitTests/WorklistItems/WorklistItemFactory.cs` — builds `WorklistItem` aggregates at various states for test setup

---

## Layer 2: Application — Commands & Handlers

One subfolder per command under `Application/WorklistItems/`.

### Step 1 — CreateWorklistItem

- [ ] `Application/WorklistItems/CreateWorklistItem/CreateWorklistItemCommand.cs`
  - Properties: `WorklistItemId`, `SampleId`, `PatientId` (all `Guid`), `SampleBarcode`, `ExamCode` (both `string`), `CreatedAt` (`DateTime`)
  - Extends `CommandBase<Guid>`
- [ ] `Application/WorklistItems/CreateWorklistItem/CreateWorklistItemCommandHandler.cs`
  - Calls `WorklistItem.Create(...)`, persists via `IAggregateStore.Start()`
  - Returns `command.WorklistItemId`

### Step 2 — RecordAnalysisResult

- [ ] `Application/WorklistItems/RecordAnalysisResult/RecordAnalysisResultCommand.cs`
  - Properties: `WorklistItemId (Guid)`, `ResultValue (string)`, `AnalystId (Guid)`, `RecordedAt (DateTime)`
  - Extends `CommandBase`
- [ ] `Application/WorklistItems/RecordAnalysisResult/RecordAnalysisResultCommandHandler.cs`
  - Loads via `new WorklistItemId(command.WorklistItemId)`, calls `RecordResult(...)`, saves via `AppendChanges`

### Step 3 — GenerateReport

- [ ] `Application/WorklistItems/GenerateReport/GenerateReportCommand.cs`
  - Properties: `WorklistItemId (Guid)`, `ReportPath (string)`, `GeneratedAt (DateTime)`
  - Extends `CommandBase`
- [ ] `Application/WorklistItems/GenerateReport/GenerateReportCommandHandler.cs`
  - Loads, calls `GenerateReport(...)`, saves

### Step 4 — CompleteWorklistItem

- [ ] `Application/WorklistItems/CompleteWorklistItem/CompleteWorklistItemCommand.cs`
  - Properties: `WorklistItemId (Guid)`, `CompletionType (string)`, `CompletedAt (DateTime)`
  - Extends `CommandBase`
- [ ] `Application/WorklistItems/CompleteWorklistItem/CompleteWorklistItemCommandHandler.cs`
  - Loads, calls `Complete(...)`, saves

---

## Layer 3: Application — Notifications

### Notification files

- [ ] `Application/WorklistItems/CreateWorklistItem/WorklistItemCreatedNotification.cs`
- [ ] `Application/WorklistItems/CreateWorklistItem/WorklistItemCreatedPublishEventNotificationHandler.cs`
  — publishes `WorklistItemCreatedIntegrationEvent` via `IEventsBus`
- [ ] `Application/WorklistItems/RecordAnalysisResult/AnalysisResultRecordedNotification.cs`
- [ ] `Application/WorklistItems/GenerateReport/ReportGeneratedNotification.cs`
- [ ] `Application/WorklistItems/CompleteWorklistItem/WorklistItemCompletedNotification.cs`
- [ ] `Application/WorklistItems/CompleteWorklistItem/WorklistItemCompletedPublishEventNotificationHandler.cs`
  — publishes `WorklistItemCompletedIntegrationEvent` via `IEventsBus`

---

## Layer 4: Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable, JSON-serializable.

- [ ] `IntegrationEvents/WorklistItemCreatedIntegrationEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `PatientId (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`
- [ ] `IntegrationEvents/WorklistItemCompletedIntegrationEvent.cs`
  - Properties: `WorklistItemId (Guid)`, `SampleId (Guid)`, `ExamCode (string)`, `CompletionType (string)`

---

## Layer 5: Infrastructure Wiring

### DomainEventTypeMappings

- [ ] `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`
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

- [ ] `Infrastructure/Configurations/LabAnalysisStartup.cs`
  - Populate BiMap with all 4 notification mappings (4/4):

```csharp
notificationsBiMap.Add("WorklistItemCreatedNotification",      typeof(WorklistItemCreatedNotification));
notificationsBiMap.Add("AnalysisResultRecordedNotification",   typeof(AnalysisResultRecordedNotification));
notificationsBiMap.Add("ReportGeneratedNotification",          typeof(ReportGeneratedNotification));
notificationsBiMap.Add("WorklistItemCompletedNotification",    typeof(WorklistItemCompletedNotification));
```

### EventsBus Subscription

- [ ] `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs`
  - Register subscription to `SampleCollectedIntegrationEvent` → `SampleCollectedIntegrationEventHandler`
- [ ] `Infrastructure/Configurations/EventsBus/SampleCollectedIntegrationEventHandler.cs`
  - Implements `IIntegrationEventHandler<SampleCollectedIntegrationEvent>`
  - Iterates `integrationEvent.ExamCodes` — dispatches **one `CreateWorklistItemCommand` per exam code**, each with its own `Guid.NewGuid()` as `WorklistItemId`
  - Example:
    ```csharp
    foreach (var examCode in integrationEvent.ExamCodes)
    {
        await _commandsScheduler.EnqueueAsync(new CreateWorklistItemCommand(
            Guid.NewGuid(),
            integrationEvent.SampleId,
            integrationEvent.SampleBarcode,
            examCode,
            integrationEvent.PatientId,
            integrationEvent.OccurredAt));
    }
    ```

### Module Facade

- [ ] `ILabAnalysisModule.cs` — already scaffolded, no changes needed
- [ ] `LabAnalysisModule.cs` — already scaffolded, no changes needed

---

## Layer 6: Read Model — WorklistItemDetails

### Database Migration

- [ ] `src/HC.LIS/HC.LIS.Database/LabAnalysis/20260326120400_LabAnalysisModule_AddTableWorklistItemDetails.cs`

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

- [ ] `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsDto.cs`
  - All columns above as public properties
- [ ] `Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQuery.cs`
  - Property: `WorklistItemId (Guid)`, extends `QueryBase<WorklistItemDetailsDto>`
- [ ] `Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQueryHandler.cs`
  - Dapper SELECT from `lab_analysis.worklist_item_details WHERE id = @worklistItemId`
- [ ] `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsProjector.cs`
  - `When(WorklistItemCreatedDomainEvent)` → INSERT, Status = `"Pending"`
  - `When(AnalysisResultRecordedDomainEvent)` → UPDATE ResultValue, Status = `"ResultReceived"`
  - `When(ReportGeneratedDomainEvent)` → UPDATE ReportPath, Status = `"ReportGenerated"`
  - `When(WorklistItemCompletedDomainEvent)` → UPDATE Status = `"Completed"`, CompletionType, CompletedAt
  - `When(IDomainEvent)` → no-op

### Notification Projection Handlers (co-located with commands)

- [ ] `Application/WorklistItems/CreateWorklistItem/WorklistItemCreatedNotificationProjection.cs`
- [ ] `Application/WorklistItems/RecordAnalysisResult/AnalysisResultRecordedNotificationProjection.cs`
- [ ] `Application/WorklistItems/GenerateReport/ReportGeneratedNotificationProjection.cs`
- [ ] `Application/WorklistItems/CompleteWorklistItem/WorklistItemCompletedNotificationProjection.cs`

---

## Layer 7: Integration Tests

Location: `Tests/IntegrationTests/WorklistItems/`
Pattern: command → `GetEventually(probe, 15000)` on `WorklistItemDetails` read model.

### Test Infrastructure

- [ ] `Tests/IntegrationTests/WorklistItems/GetWorklistItemDetailsFromLabAnalysisProbe.cs` — `IProbe<WorklistItemDetailsDto>`
- [ ] `Tests/IntegrationTests/TestBase.cs`
  - `ClearDatabase()` truncates `lab_analysis.worklist_item_details`
  - Add `GetEventually()` static method (identical pattern to SampleCollection/TestOrders)
  - `IDisposable` calls `LabAnalysisStartup.Stop()`

### Test Methods

- [ ] `Tests/IntegrationTests/WorklistItems/WorklistItemTests.cs`

| Test Method | Command Sent | Assertion |
|---|---|---|
| `CreateWorklistItemIsSuccessful` | `CreateWorklistItemCommand` | `WorklistItemDetails.Status == "Pending"`, `SampleBarcode`, `ExamCode`, `PatientId` set |
| `RecordAnalysisResultIsSuccessful` | `RecordAnalysisResultCommand` | `Status == "ResultReceived"`, `ResultValue` set |
| `GenerateReportIsSuccessful` | `GenerateReportCommand` | `Status == "ReportGenerated"`, `ReportPath` set |
| `CompleteWorklistItemIsSuccessful` | `CompleteWorklistItemCommand` | `Status == "Completed"`, `CompletionType` set, `CompletedAt` set |

---

## Verification Checklist

- [ ] `dotnet build` — zero warnings (`TreatWarningsAsErrors=true`)
- [ ] `dotnet test` UnitTests — all tests pass
- [ ] `dotnet test` IntegrationTests — all 4 tests pass
- [ ] SampleCollection `dotnet test` UnitTests — still 14 passed (after prerequisite change)
- [ ] SampleCollection `dotnet test` IntegrationTests — still 6 passed (after prerequisite change)

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
