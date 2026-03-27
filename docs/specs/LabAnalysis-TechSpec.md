# Technical Spec: LabAnalysis Module

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-03-26
**PRD Reference:** [docs/prd/LabAnalysis.md](../prd/LabAnalysis.md)

---

## 1. Overview

The LabAnalysis module manages the analysis worklist lifecycle — from receiving a sample-collected notification, through clinical analyzer communication, to releasing a digital report and notifying TestOrders of completion.

**Aggregate root:** `WorklistItem`
**Schema:** `lab_analysis`

---

## 2. Aggregate: `WorklistItem`

### 2.1 Identity

`WorklistItemId` — typed ID wrapping `Guid`, string-keyed in Marten (`StreamIdentity.AsString`).

### 2.2 State Machine

```
Pending → ResultReceived → ReportGenerated → Completed
```

| Status | Meaning |
|---|---|
| `Pending` | Worklist item created; awaiting result from clinical analyzer |
| `ResultReceived` | Result recorded; report not yet generated |
| `ReportGenerated` | PDF report stored; item not yet formally completed |
| `Completed` | Notification sent to TestOrders; no further state changes allowed |

### 2.3 Domain Methods & Events

| Method | Business Rule(s) | Domain Event Emitted |
|---|---|---|
| `Create(Guid worklistItemId, Guid sampleId, string sampleBarcode, string examCode, Guid patientId, DateTime createdAt)` | — | `WorklistItemCreatedDomainEvent` |
| `RecordResult(string resultValue, Guid analystId, DateTime recordedAt)` | `CannotRecordResultForNonPendingWorklistItemRule` | `AnalysisResultRecordedDomainEvent` |
| `GenerateReport(string reportPath, DateTime generatedAt)` | `CannotGenerateReportWithoutResultRule` | `ReportGeneratedDomainEvent` |
| `Complete(string completionType, DateTime completedAt)` | `CannotCompleteWorklistItemWithoutReportRule` | `WorklistItemCompletedDomainEvent` |

### 2.4 Business Rules

| Class | Invariant |
|---|---|
| `CannotRecordResultForNonPendingWorklistItemRule` | Status must be `Pending` to record a result |
| `CannotGenerateReportWithoutResultRule` | Status must be `ResultReceived` to generate a report |
| `CannotCompleteWorklistItemWithoutReportRule` | Status must be `ReportGenerated` to complete the item |

### 2.5 Domain Events (fields)

**`WorklistItemCreatedDomainEvent`**
- `WorklistItemId` (Guid), `SampleId` (Guid), `SampleBarcode` (string), `ExamCode` (string), `PatientId` (Guid), `CreatedAt` (DateTime)

**`AnalysisResultRecordedDomainEvent`**
- `WorklistItemId` (Guid), `ResultValue` (string), `AnalystId` (Guid), `RecordedAt` (DateTime)

**`ReportGeneratedDomainEvent`**
- `WorklistItemId` (Guid), `ReportPath` (string), `GeneratedAt` (DateTime)

**`WorklistItemCompletedDomainEvent`**
- `WorklistItemId` (Guid), `SampleId` (Guid), `ExamCode` (string), `CompletionType` (string — `"PartialComplete"` or `"Complete"`), `CompletedAt` (DateTime)

---

## 3. Application Layer

### 3.1 Commands

Location: `Application/WorklistItems/{CommandName}/`

| Command | Properties | Aggregate Method Called |
|---|---|---|
| `CreateWorklistItemCommand` | `WorklistItemId (Guid)`, `SampleId (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`, `PatientId (Guid)`, `CreatedAt (DateTime)` — extends `CommandBase<Guid>` | `WorklistItem.Create(...)` via `IAggregateStore.Start()` |
| `RecordAnalysisResultCommand` | `WorklistItemId (Guid)`, `ResultValue (string)`, `AnalystId (Guid)`, `RecordedAt (DateTime)` — extends `CommandBase` | `WorklistItem.RecordResult(...)` via `AppendChanges` |
| `GenerateReportCommand` | `WorklistItemId (Guid)`, `ReportPath (string)`, `GeneratedAt (DateTime)` — extends `CommandBase` | `WorklistItem.GenerateReport(...)` via `AppendChanges` |
| `CompleteWorklistItemCommand` | `WorklistItemId (Guid)`, `CompletionType (string)`, `CompletedAt (DateTime)` — extends `CommandBase` | `WorklistItem.Complete(...)` via `AppendChanges` |

### 3.2 Notifications

One notification per domain event. Notifications are co-located in the same folder as their triggering command.

| Notification | Co-located With | Integration Event? |
|---|---|---|
| `WorklistItemCreatedNotification` | `CreateWorklistItem/` | **Yes** — emits `WorklistItemCreatedIntegrationEvent` to clinical analyzer |
| `AnalysisResultRecordedNotification` | `RecordAnalysisResult/` | No |
| `ReportGeneratedNotification` | `GenerateReport/` | No |
| `WorklistItemCompletedNotification` | `CompleteWorklistItem/` | **Yes** — emits `WorklistItemCompletedIntegrationEvent` to TestOrders |

### 3.3 Notification Projection Handlers

Projections are co-located with their command folder (not in the read model folder).

| Projection Class | Co-located With | Read Model Updated |
|---|---|---|
| `WorklistItemCreatedNotificationProjection` | `CreateWorklistItem/` | `WorklistItemDetails` — INSERT |
| `AnalysisResultRecordedNotificationProjection` | `RecordAnalysisResult/` | `WorklistItemDetails` — UPDATE ResultValue, Status |
| `ReportGeneratedNotificationProjection` | `GenerateReport/` | `WorklistItemDetails` — UPDATE ReportPath, Status |
| `WorklistItemCompletedNotificationProjection` | `CompleteWorklistItem/` | `WorklistItemDetails` — UPDATE Status, CompletionType, CompletedAt |

---

## 4. Integration Events

### 4.1 Inbound — Subscription

**`SampleCollectedIntegrationEvent`** (emitted by SampleCollection module)

> **Cross-module dependency:** The current `SampleCollectedIntegrationEvent` only carries `CollectionRequestId` and `SampleId`. LabAnalysis requires `SampleBarcode`, `PatientId`, and — critically — **a collection of exam codes**, because a single collected sample may have multiple exams ordered on it, each of which produces a distinct analyte and must become its own `WorklistItem`. The SampleCollection module's `SampleCollectedIntegrationEvent` must be enriched with `SampleBarcode (string)`, `PatientId (Guid)`, and `ExamCodes (IReadOnlyCollection<string>)` before this subscription can be activated.

Subscription handler: `SampleCollectedIntegrationEventHandler` (implements `IIntegrationEventHandler<SampleCollectedIntegrationEvent>`)
→ Iterates `integrationEvent.ExamCodes` and dispatches **one `CreateWorklistItemCommand` per exam code**, each with a freshly generated `WorklistItemId`.

### 4.2 Outbound — Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable, JSON-serializable.

**`WorklistItemCreatedIntegrationEvent`**
- `WorklistItemId` (Guid) — the LabAnalysis identifier for this item
- `PatientId` (Guid)
- `SampleBarcode` (string)
- `ExamCode` (string)

> Consumed by: clinical analyzer integration layer — triggers analyzer instrument setup.

**`WorklistItemCompletedIntegrationEvent`**
- `WorklistItemId` (Guid)
- `SampleId` (Guid)
- `ExamCode` (string)
- `CompletionType` (string — `"PartialComplete"` or `"Complete"`)

> Consumed by: TestOrders module — triggers order partial/full completion logic.

---

## 5. Read Model: `WorklistItemDetails`

Location: `Application/WorklistItems/GetWorklistItemDetails/`

### 5.1 Table Schema

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `WorklistItemCreatedDomainEvent` |
| `SampleId` | UUID | `WorklistItemCreatedDomainEvent` |
| `SampleBarcode` | VARCHAR(255) | `WorklistItemCreatedDomainEvent` |
| `ExamCode` | VARCHAR(255) | `WorklistItemCreatedDomainEvent` |
| `PatientId` | UUID | `WorklistItemCreatedDomainEvent` |
| `Status` | VARCHAR(50) | All state transitions |
| `ResultValue` | TEXT NULL | `AnalysisResultRecordedDomainEvent` |
| `ReportPath` | VARCHAR(500) NULL | `ReportGeneratedDomainEvent` |
| `CompletionType` | VARCHAR(50) NULL | `WorklistItemCompletedDomainEvent` |
| `CreatedAt` | TIMESTAMPTZ | `WorklistItemCreatedDomainEvent` |
| `CompletedAt` | TIMESTAMPTZ NULL | `WorklistItemCompletedDomainEvent` |

### 5.2 Application Files

- `WorklistItemDetailsDto.cs`
- `GetWorklistItemDetailsQuery.cs`
- `GetWorklistItemDetailsQueryHandler.cs` — Dapper SELECT
- `WorklistItemDetailsProjector.cs`
  - `When(WorklistItemCreatedDomainEvent)` → INSERT, Status = `"Pending"`
  - `When(AnalysisResultRecordedDomainEvent)` → UPDATE ResultValue, Status = `"ResultReceived"`
  - `When(ReportGeneratedDomainEvent)` → UPDATE ReportPath, Status = `"ReportGenerated"`
  - `When(WorklistItemCompletedDomainEvent)` → UPDATE Status = `"Completed"`, CompletionType, CompletedAt
  - `When(IDomainEvent)` → fall-through (no-op)

---

## 6. Infrastructure Wiring

### 6.1 DomainEventTypeMappings

Register all 4 domain events in `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`:

```csharp
options.Events.AddEventType<WorklistItemCreatedDomainEvent>();
options.Events.AddEventType<AnalysisResultRecordedDomainEvent>();
options.Events.AddEventType<ReportGeneratedDomainEvent>();
options.Events.AddEventType<WorklistItemCompletedDomainEvent>();
```

### 6.2 LabAnalysisStartup — OutboxModule BiMap

Register all 4 notification type mappings in `Infrastructure/Configurations/LabAnalysisStartup.cs`:

```csharp
notificationsBiMap.Add("WorklistItemCreatedNotification",       typeof(WorklistItemCreatedNotification));
notificationsBiMap.Add("AnalysisResultRecordedNotification",    typeof(AnalysisResultRecordedNotification));
notificationsBiMap.Add("ReportGeneratedNotification",           typeof(ReportGeneratedNotification));
notificationsBiMap.Add("WorklistItemCompletedNotification",     typeof(WorklistItemCompletedNotification));
```

### 6.3 EventsBus Subscription

Register `SampleCollectedIntegrationEventHandler` in the `EventsBusModule` (or `EventsBusStartup`) to subscribe to `SampleCollectedIntegrationEvent`.

### 6.4 Module Facade

`ILabAnalysisModule` and `LabAnalysisModule` already scaffolded — generic dispatcher pattern, no changes needed.

---

## 7. Database Migrations

Location: `src/HC.LIS/HC.LIS.Database/LabAnalysis/`

| File | Purpose |
|---|---|
| `20260325120000_LabAnalysisModule_AddSchemaLabAnalysis.cs` | ✅ Already exists |
| `20260325120100_LabAnalysisModule_AddTableInboxMessages.cs` | ✅ Already exists |
| `20260325120200_LabAnalysisModule_AddTableInternalCommands.cs` | ✅ Already exists |
| `20260325120300_LabAnalysisModule_AddTableOutboxMessages.cs` | ✅ Already exists |
| `20260326120400_LabAnalysisModule_AddTableWorklistItemDetails.cs` | ⬜ To be created |

---

## 8. Unit Tests

Location: `Tests/UnitTests/WorklistItems/WorklistItemTests.cs`
Pattern: Arrange–Act–Assert, `AssertPublishedDomainEvent<T>()` on aggregate, FluentAssertions.

| Test | Asserts |
|---|---|
| `CreateWorklistItemIsSuccessful` | `WorklistItemCreatedDomainEvent` raised with correct fields |
| `RecordAnalysisResultIsSuccessful` | `AnalysisResultRecordedDomainEvent` raised |
| `GenerateReportIsSuccessful` | `ReportGeneratedDomainEvent` raised |
| `CompleteWorklistItemIsSuccessful` | `WorklistItemCompletedDomainEvent` raised |
| `RecordResultThrowsWhenNotPending` | `BaseBusinessRuleException` with `CannotRecordResultForNonPendingWorklistItemRule` |
| `GenerateReportThrowsWhenResultNotReceived` | `BaseBusinessRuleException` with `CannotGenerateReportWithoutResultRule` |
| `CompleteThrowsWhenReportNotGenerated` | `BaseBusinessRuleException` with `CannotCompleteWorklistItemWithoutReportRule` |

---

## 9. Integration Tests

Location: `Tests/IntegrationTests/WorklistItems/`
Pattern: `TestBase.ExecuteCommandAsync(command)` → `GetEventually(probe, timeoutMs)` on projected read model.

| Test | Command Sent | Read Model Assertion |
|---|---|---|
| `CreateWorklistItemIsSuccessful` | `CreateWorklistItemCommand` | `WorklistItemDetails.Status == "Pending"`, all identity fields set |
| `RecordAnalysisResultIsSuccessful` | `RecordAnalysisResultCommand` | `Status == "ResultReceived"`, `ResultValue` set |
| `GenerateReportIsSuccessful` | `GenerateReportCommand` | `Status == "ReportGenerated"`, `ReportPath` set |
| `CompleteWorklistItemIsSuccessful` | `CompleteWorklistItemCommand` | `Status == "Completed"`, `CompletionType` set, `CompletedAt` set |

---

## 10. Open Design Decisions

| # | Decision | Options | Recommendation |
|---|---|---|---|
| 1 | `SampleCollectedIntegrationEvent` enrichment | Extend existing event vs. subscribe to `BarcodeCreatedIntegrationEvent` too | Extend `SampleCollectedIntegrationEvent` with `SampleBarcode`, `PatientId`, and `ExamCodes (IReadOnlyCollection<string>)` in SampleCollection module. A single sample can have N exams ordered; the handler fans out N `CreateWorklistItemCommand` dispatches. |
| 2 | `GenerateReportCommand` — internal vs. external command | Schedule as `InternalCommandBase` after result vs. regular external command | External command for now (simpler); promote to internal command when automation is needed |
| 3 | `CompletionType` determination | PartialComplete vs. Complete logic | At the WorklistItem level, always `Complete`; PartialComplete determined by TestOrders (out of scope for this module) |
