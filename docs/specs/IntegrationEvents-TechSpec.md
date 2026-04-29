# Technical Spec: HC.LIS.Tests.IntegrationEvents

**Status:** Draft
**Date:** 2026-04-29

---

## 1. Overview

`HC.LIS.Tests.IntegrationEvents` is the system-level test project that verifies integration events flow correctly across module boundaries. Each test exercises a real domain transition in one module and probes the projected state in a downstream module after the event has propagated asynchronously.

**Project location:** `src/HC.LIS/HC.LIS.Tests/IntegrationEvents/`
**Schema touched:** all four modules — `test_orders`, `sample_collection`, `analyzer`, `lab_analysis`

Tests run against a real PostgreSQL database. All module Startups are initialized in `TestBase.InitializeAsync()`. Each test class group has its own `TestBase` subclass that exposes the module facade instances relevant to that group.

---

## 2. Integration Flow Map

The complete cross-module event graph. Each arrow is a flow that must have at least one test.

```
TestOrders
  PUBLISHES  OrderItemAcceptedIntegrationEvent
               └─► SampleCollection: CreateCollectionRequestForOrderCommand
                                      AddExamToCollectionForOrderCommand

SampleCollection
  PUBLISHES  SampleCollectedIntegrationEvent
               ├─► TestOrders:   PlaceExamInProgressByExamIdCommand
               ├─► Analyzer:     CreateAnalyzerSampleBySampleCollectedCommand
               └─► LabAnalysis:  CreateWorklistItemCommand (one per exam)

LabAnalysis
  PUBLISHES  WorklistItemCreatedIntegrationEvent
               └─► Analyzer: AssignWorklistItemByBarcodeAndExamCodeCommand

Analyzer
  PUBLISHES  ExamResultReceivedIntegrationEvent
               └─► LabAnalysis: RecordAnalysisResultCommand

LabAnalysis
  PUBLISHES  WorklistItemCompletedIntegrationEvent
               └─► TestOrders: CompleteExamByExamIdCommand
```

---

## 3. Test Scenarios

Scenarios are grouped by the module whose command triggers the integration event chain.

### 3.1 Group A — TestOrders triggers SampleCollection

**Trigger:** `AcceptExamCommand` in TestOrders publishes `OrderItemAcceptedIntegrationEvent`.

| Scenario | Setup | Trigger Command | Expected Downstream State |
|---|---|---|---|
| `SingleExamAccepted_CreatesCollectionRequest` | Place order, request one exam | `AcceptExamCommand` | `sample_collection.CollectionRequestDetails` row exists for the `OrderId` |
| `MultipleExamsAccepted_AddsExamsToSameCollectionRequest` | Place order, request two exams, accept first | Accept second exam | Same `CollectionRequestId`; collection request has 2 exams |

**Setup chain:** `CreateOrderCommand` → `RequestExamCommand` → `AcceptExamCommand`

---

### 3.2 Group B — SampleCollection fan-out

**Trigger:** `RecordSampleCollectionCommand` in SampleCollection publishes `SampleCollectedIntegrationEvent`. This single event fans out to three modules simultaneously.

| Scenario | Setup | Trigger Command | Expected Downstream State |
|---|---|---|---|
| `SampleCollected_PlacesExamInProgressInTestOrders` | Full Group A setup; CreateBarcode | `RecordSampleCollectionCommand` | `test_orders.OrderItemDetails.Status == "InProgress"` |
| `SampleCollected_CreatesAnalyzerSample` | Same | Same | `analyzer.analyzer_sample_details` row exists for `SampleBarcode` |
| `SampleCollected_CreatesWorklistItemsInLabAnalysis` | Same | Same | One `lab_analysis.worklist_item_details` row per exam |
| `SampleCollected_AssignsWorklistItemToAnalyzerExam` | Same (depends on WorklistItemCreated → Analyzer chain) | Same | `analyzer.analyzer_sample_exam_details.worklist_item_id` populated for each exam |

**Setup chain:** Group A setup → `MarkPatientArrivedCommand` → `CreateBarcodeCommand` → `RecordSampleCollectionCommand`

> The last scenario (AssignWorklistItem) is a second-order effect: `SampleCollectedIntegrationEvent` → LabAnalysis creates `WorklistItem` → LabAnalysis publishes `WorklistItemCreatedIntegrationEvent` → Analyzer assigns it. Both hops are validated in the same test.

---

### 3.3 Group C — Analyzer triggers LabAnalysis

**Trigger:** Receiving an exam result in Analyzer publishes `ExamResultReceivedIntegrationEvent`, which LabAnalysis subscribes to directly and maps to `RecordAnalysisResultCommand`.

| Scenario | Setup | Trigger Command | Expected Downstream State |
|---|---|---|---|
| `ExamResultReceived_RecordsAnalysisResultInLabAnalysis` | Full Group B setup | `ForwardRawResultCommand` (Analyzer) | `lab_analysis.worklist_item_analyte_results` row exists; `worklist_item_details.Status == "ResultReceived"` |

**Setup chain:** Group B setup → `ForwardRawResultCommand`

---

### 3.4 Group D — LabAnalysis triggers TestOrders

**Trigger:** `CompleteWorklistItemCommand` in LabAnalysis publishes `WorklistItemCompletedIntegrationEvent`, which TestOrders subscribes to and maps to `CompleteExamByExamIdCommand`.

| Scenario | Setup | Trigger Command | Expected Downstream State |
|---|---|---|---|
| `WorklistItemCompleted_CompletesExamInTestOrders` | Full Group C setup | `CompleteWorklistItemCommand` (LabAnalysis) | `test_orders.OrderItemDetails.Status == "Completed"` |

**Setup chain:** Group C setup → `GenerateReportCommand` → `CompleteWorklistItemCommand`

---

### 3.5 End-to-End

| Scenario | Description |
|---|---|
| `FullLaboratoryWorkflow_FromOrderAcceptanceToCompletion` | Exercises every flow in sequence: accept exam → collect sample → receive result → complete worklist item → exam completed |

---

## 4. Test Infrastructure (Scaffolded)

All infrastructure files already exist at `src/HC.LIS/HC.LIS.Tests/IntegrationEvents/`:

| File | Purpose |
|---|---|
| `TestBase.cs` | Abstract `IAsyncLifetime` base — reads connection string, clears DB, initializes all 4 module Startups |
| `ExecutionContextMock.cs` | `IExecutionContextAccessor` with settable `UserId` |
| `DatabaseCleaner.cs` | Dapper deletes for all 4 schemas; child rows deleted before parent rows |
| `IntegrationTestAssert.cs` | `AssertEventually(IProbe, int timeoutMs)` wrapping `Poller`; `AssertBrokenRule<TRule>(Action)` |

`TestBase` exposes `ConnectionString` and `ExecutionContext`. Each scenario group extends `TestBase` and adds its own module facade properties.

---

## 5. Probe Design

Each probe implements `IProbe` from `HC.Core.IntegrationTests`. Probes are placed in `Probes/`.

**Naming convention:** `Get{ObservableState}From{TargetModule}Probe`

### 5.1 Probe inventory

| Probe Class | Module Queried | Satisfied When |
|---|---|---|
| `GetCollectionRequestFromSampleCollectionProbe` | `ISampleCollectionModule` | `CollectionRequestDetails` row exists for expected `CollectionRequestId` |
| `GetExamInProgressFromTestOrdersProbe` | `ITestOrdersModule` | `OrderItemDetails.Status == "InProgress"` for expected `OrderItemId` |
| `GetAnalyzerSampleFromAnalyzerProbe` | `IAnalyzerModule` | `analyzer_sample_details` row exists for expected `SampleBarcode` |
| `GetWorklistItemFromLabAnalysisProbe` | `ILabAnalysisModule` | `worklist_item_details` row exists for expected exam code and sample |
| `GetWorklistItemAssignedFromAnalyzerProbe` | `IAnalyzerModule` | `analyzer_sample_exam_details.worklist_item_id` is not null for expected exam |
| `GetAnalysisResultFromLabAnalysisProbe` | `ILabAnalysisModule` | `worklist_item_analyte_results` row exists for expected `WorklistItemId` |
| `GetExamCompletedFromTestOrdersProbe` | `ITestOrdersModule` | `OrderItemDetails.Status == "Completed"` for expected `OrderItemId` |

### 5.2 Probe pattern

```csharp
public sealed class GetCollectionRequestFromSampleCollectionProbe(
    Guid expectedCollectionRequestId,
    ISampleCollectionModule module) : IProbe
{
    private CollectionRequestDetailsDto? _result;

    public bool IsSatisfied() => _result is not null
        && _result.Id == _expectedCollectionRequestId;

    public async Task SampleAsync()
    {
        try { _result = await module.ExecuteQueryAsync(
            new GetCollectionRequestDetailsQuery(_expectedCollectionRequestId)); }
        catch { }
    }

    public string DescribeFailureTo()
        => $"CollectionRequest {_expectedCollectionRequestId} not found in SampleCollection";
}
```

All probes follow this structure: catch all exceptions in `SampleAsync` (the state may not exist yet), check nullability and expected field values in `IsSatisfied`.

---

## 6. Folder and File Structure

```
src/HC.LIS/HC.LIS.Tests/IntegrationEvents/
├── HC.LIS.Tests.IntegrationEvents.csproj
├── TestBase.cs                              ← already exists
├── ExecutionContextMock.cs                  ← already exists
├── DatabaseCleaner.cs                       ← already exists
├── IntegrationTestAssert.cs                 ← already exists
│
├── Probes/
│   ├── GetCollectionRequestFromSampleCollectionProbe.cs
│   ├── GetExamInProgressFromTestOrdersProbe.cs
│   ├── GetAnalyzerSampleFromAnalyzerProbe.cs
│   ├── GetWorklistItemFromLabAnalysisProbe.cs
│   ├── GetWorklistItemAssignedFromAnalyzerProbe.cs
│   ├── GetAnalysisResultFromLabAnalysisProbe.cs
│   └── GetExamCompletedFromTestOrdersProbe.cs
│
├── TestOrders/
│   ├── TestBase.cs          ← exposes ITestOrdersModule, ISampleCollectionModule
│   └── OrderItemAcceptedFlowTests.cs
│
├── SampleCollection/
│   ├── TestBase.cs          ← exposes ITestOrdersModule, ISampleCollectionModule,
│   │                           IAnalyzerModule, ILabAnalysisModule
│   └── SampleCollectedFlowTests.cs
│
├── Analyzer/
│   ├── TestBase.cs          ← exposes ITestOrdersModule, ISampleCollectionModule,
│   │                           IAnalyzerModule, ILabAnalysisModule
│   └── ExamResultReceivedFlowTests.cs
│
└── LabAnalysis/
    ├── TestBase.cs          ← exposes ITestOrdersModule, ILabAnalysisModule
    └── WorklistItemCompletedFlowTests.cs
```

Each scenario group's `TestBase` subclass:
- Extends the shared `TestBase`
- Declares `protected` module facade properties (e.g., `protected ITestOrdersModule TestOrdersModule`)
- Overrides `InitializeAsync` to call `base.InitializeAsync()` and then instantiate the facades

---

## 7. DatabaseCleaner Table Inventory

The existing `DatabaseCleaner.ClearAllAsync()` deletes from all four schemas. Child rows are deleted before parent rows to satisfy FK constraints.

| Schema | Tables (delete order) |
|---|---|
| `test_orders` | `InboxMessages`, `InternalCommands`, `OutboxMessages`, `OrderItemDetails`, `OrderDetails`, `mt_events`, `mt_streams` |
| `sample_collection` | `InboxMessages`, `InternalCommands`, `OutboxMessages`, `SampleDetails`, `CollectionRequestDetails`, `mt_events`, `mt_streams` |
| `analyzer` | `InboxMessages`, `InternalCommands`, `OutboxMessages`, `analyzer_sample_exam_details`, `analyzer_sample_details`, `mt_doc_deadletterevent`, `mt_event_progression`, `mt_events`, `mt_streams` |
| `lab_analysis` | `InboxMessages`, `InternalCommands`, `OutboxMessages`, `worklist_item_analyte_results`, `signed_report_details`, `worklist_item_details`, `mt_events`, `mt_streams` |

> **Note:** Exact table names must be verified against the FluentMigrator migrations in `src/HC.LIS/HC.LIS.Database/` before the first test run. The `DatabaseCleaner` already has these table names — fix any mismatches discovered at that point.

---

## 8. Probe Timeouts

Integration event propagation goes through the Outbox → EventBus → Inbox → Quartz job pipeline. Use consistent timeouts across all probes:

| Scenario type | Recommended timeout |
|---|---|
| Single-hop (one event, one command) | 15 000 ms |
| Two-hop (e.g., SampleCollected → WorklistItemCreated → AssignWorklistItem) | 25 000 ms |
| End-to-end (full workflow) | 60 000 ms |

---

## 9. Open Design Decisions

| # | Decision | Resolution |
|---|---|---|
| 1 | Module facade query methods for probes | Verify `GetCollectionRequestDetailsQuery`, `GetOrderItemDetailsQuery`, `GetAnalyzerSampleDetailsQuery`, `GetWorklistItemDetailsQuery` exist on each module facade before writing probes. Use direct Dapper queries on `ConnectionString` as fallback if a facade query is not exposed. |
| 2 | `ForwardRawResultCommand` signature in Analyzer | Confirm whether raw bytes or a parsed DTO are passed. If raw bytes (as in TcpMessage integration tests), construct a minimal valid ORU^R01 payload in the test helper. |
| 3 | `GenerateReportCommand` prerequisite for Group D | LabAnalysis requires `ReportGenerated` status before `CompleteWorklistItem`. Confirm whether `GenerateReportCommand` requires a real file path or accepts a stub string (e.g., `"test-report.pdf"`). |
| 4 | Quartz polling interval in test Startups | All Startups currently pass `eventBus: null`. Confirm that internal commands still execute via Quartz when no event bus is wired (i.e., the Outbox/Inbox pipeline is self-contained). |
