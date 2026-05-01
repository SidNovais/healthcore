# Implementation Tasks: HC.LIS.Tests.IntegrationEvents

**Tech Spec:** [docs/specs/IntegrationEvents-TechSpec.md](./IntegrationEvents-TechSpec.md)
**Date:** 2026-04-29

> **Workflow rule:** After implementing each task, mark it as done (`- [x]`) in this file before moving on.

---

## Prerequisites

1. All four module Startups (`TestOrdersStartup`, `SampleCollectionStartup`, `AnalyzerStartup`, `LabAnalysisStartup`) must be fully implemented and their integration tests green.
2. `ASPNETCORE_HCLIS_IntegrationTests_ConnectionString` environment variable must be set pointing to a running PostgreSQL instance with all migrations applied.
3. The scaffolded infrastructure (`TestBase`, `ExecutionContextMock`, `DatabaseCleaner`, `IntegrationTestAssert`) already exists — do not re-create it.
4. Resolve Open Design Decisions 1–4 from the tech spec before writing any test that depends on them.

---

## Task List

### Phase 1: Resolve Open Design Decisions

- [x] **Task 1.1** — Verify probe query methods on each module facade
  - All 4 queries exist. 2 probes use Dapper fallback (CollectionRequest by PatientId; WorklistItem by barcode+examCode). GetAnalyzerSample uses `GetSampleInfoByBarcodeQuery`. See plan for details.

- [x] **Task 1.2** — Verify `ForwardRawResultCommand` signature in Analyzer
  - Takes `byte[]` rawResultPayload (HL7 ORU^R01, pipe-delimited). `Hl7MessageBuilder` to be created in `Analyzer/` folder in Phase 5.

- [x] **Task 1.3** — Verify `GenerateReportCommand` accepts stub path
  - No file system validation. `"test-report.pdf"` works. Command extends `InternalCommandBase` (implements `ICommand`) — callable directly on `ILabAnalysisModule.ExecuteCommandAsync`.

- [x] **Task 1.4** — Verify Quartz executes internal commands with `eventBus: null`
  - Confirmed via code review. `InMemoryEventBusClient` registered when null; all three Quartz jobs run on 2-second intervals regardless.

---

### Phase 2: Probes

- [x] **Task 2.1** — `GetCollectionRequestFromSampleCollectionProbe`
  - Dapper query on `sample_collection."CollectionRequestDetails"` by `PatientId`. `IsSatisfied`: not null.

- [x] **Task 2.2** — `GetExamInProgressFromTestOrdersProbe`
  - Facade `GetOrderItemDetailsQuery`. Default predicate `Status == "InProgress"`, overridable via `satisfiedWhen`.

- [x] **Task 2.3** — `GetAnalyzerSampleFromAnalyzerProbe`
  - Facade `GetSampleInfoByBarcodeQuery(barcode)`. `IsSatisfied`: not null.

- [x] **Task 2.4** — `GetWorklistItemFromLabAnalysisProbe`
  - Dapper query on `lab_analysis.worklist_item_details` by barcode + examCode. `IsSatisfied`: not null.

- [x] **Task 2.5** — `GetWorklistItemAssignedFromAnalyzerProbe`
  - Facade `GetSampleInfoByBarcodeQuery` — uses `SampleInfoDto.Exams` collection directly (has `WorklistItemId` per exam). `IsSatisfied`: exam with matching mnemonic has non-null `WorklistItemId`.

- [x] **Task 2.6** — `GetAnalysisResultFromLabAnalysisProbe`
  - Facade `GetWorklistItemDetailsQuery`. `IsSatisfied`: `Status == "ResultReceived"`.

- [x] **Task 2.7** — `GetExamCompletedFromTestOrdersProbe`
  - Facade `GetOrderItemDetailsQuery`. `IsSatisfied`: `Status == "Completed"`.

---

### Phase 3: Group A — TestOrders → SampleCollection

- [x] **Task 3.1** — `TestOrders/TestBase.cs`
  - **Creates:** `TestOrders/TestBase.cs`
  - Extends shared `TestBase`; adds `protected ITestOrdersModule TestOrdersModule` and `protected ISampleCollectionModule SampleCollectionModule`
  - `InitializeAsync`: calls `base.InitializeAsync()`, then instantiates both module facades
  - `DisposeAsync`: calls `base.DisposeAsync()`

- [x] **Task 3.2** — `OrderItemAcceptedFlowTests` — single exam
  - **Creates:** `TestOrders/OrderItemAcceptedFlowTests.cs`
  - **Test:** `SingleExamAccepted_CreatesCollectionRequest`
    1. Place order in TestOrders (`CreateOrderCommand`)
    2. Request exam (`RequestExamCommand`) → `OrderItemId`
    3. Accept exam (`AcceptExamCommand`)
    4. `AssertEventually(new GetCollectionRequestFromSampleCollectionProbe(OrderId, SampleCollectionModule), 15_000)`
  - **Verify:** Test fails (no handler wired) → then verify it passes with DB running

- [x] **Task 3.3** — `OrderItemAcceptedFlowTests` — multiple exams
  - **Modifies:** `TestOrders/OrderItemAcceptedFlowTests.cs`
  - **Test:** `MultipleExamsAccepted_AddsExamsToSameCollectionRequest`
    1. Place order, request two exams, accept first → CollectionRequest created
    2. Accept second exam
    3. Probe: same `CollectionRequestId` has second exam

---

### Phase 4: Group B — SampleCollection fan-out

- [x] **Task 4.1** — `SampleCollection/TestBase.cs`
  - **Creates:** `SampleCollection/TestBase.cs`
  - Extends shared `TestBase`; exposes all four module facades
  - Includes protected helper `SetupCollectedSampleAsync(string barcode, string examMnemonic)` that runs the full Group A chain + `MarkPatientArrived` + `CreateBarcode` + `RecordSampleCollection`, returning `(OrderId, OrderItemId, SampleId, Barcode)`

- [x] **Task 4.2** — `SampleCollectedFlowTests` — TestOrders receives InProgress
  - **Creates:** `SampleCollection/SampleCollectedFlowTests.cs`
  - **Test:** `SampleCollected_PlacesExamInProgressInTestOrders`
    1. `SetupCollectedSampleAsync(...)`
    2. `AssertEventually(new GetExamInProgressFromTestOrdersProbe(OrderItemId, TestOrdersModule), 15_000)`

- [x] **Task 4.3** — `SampleCollectedFlowTests` — Analyzer receives AnalyzerSample
  - **Modifies:** `SampleCollection/SampleCollectedFlowTests.cs`
  - **Test:** `SampleCollected_CreatesAnalyzerSample`
    1. `SetupCollectedSampleAsync(...)`
    2. `AssertEventually(new GetAnalyzerSampleFromAnalyzerProbe(Barcode, AnalyzerModule), 15_000)`

- [x] **Task 4.4** — `SampleCollectedFlowTests` — LabAnalysis receives WorklistItem
  - **Modifies:** `SampleCollection/SampleCollectedFlowTests.cs`
  - **Test:** `SampleCollected_CreatesWorklistItemInLabAnalysis`
    1. `SetupCollectedSampleAsync(...)`
    2. `AssertEventually(new GetWorklistItemFromLabAnalysisProbe(Barcode, ExamMnemonic, LabAnalysisModule), 15_000)`

- [x] **Task 4.5** — `SampleCollectedFlowTests` — Analyzer exam gets WorklistItemId assigned
  - **Modifies:** `SampleCollection/SampleCollectedFlowTests.cs`
  - **Test:** `SampleCollected_AssignsWorklistItemToAnalyzerExam`
    1. `SetupCollectedSampleAsync(...)`
    2. `AssertEventually(new GetWorklistItemAssignedFromAnalyzerProbe(Barcode, ExamMnemonic, AnalyzerModule), 25_000)` ← two-hop timeout

---

### Phase 5: Group C — Analyzer → LabAnalysis

- [ ] **Task 5.1** — `Analyzer/TestBase.cs`
  - **Creates:** `Analyzer/TestBase.cs`
  - Extends shared `TestBase`; exposes all four module facades
  - Includes protected helper `SetupExamResultReadyAsync(string barcode, string examMnemonic)` — runs full Group B chain (using `SetupCollectedSampleAsync`), then waits for `GetWorklistItemFromLabAnalysisProbe` before returning `WorklistItemId`
  - Includes optional `Hl7MessageBuilder.BuildOruR01(...)` helper if `ForwardRawResultCommand` takes raw bytes (from Task 1.2)

- [ ] **Task 5.2** — `ExamResultReceivedFlowTests`
  - **Creates:** `Analyzer/ExamResultReceivedFlowTests.cs`
  - **Test:** `ExamResultReceived_RecordsAnalysisResultInLabAnalysis`
    1. `SetupExamResultReadyAsync(barcode, examMnemonic)` → `WorklistItemId`
    2. Execute `ForwardRawResultCommand` (or equivalent) on `AnalyzerModule`
    3. `AssertEventually(new GetAnalysisResultFromLabAnalysisProbe(WorklistItemId, LabAnalysisModule), 15_000)`

---

### Phase 6: Group D — LabAnalysis → TestOrders

- [ ] **Task 6.1** — `LabAnalysis/TestBase.cs`
  - **Creates:** `LabAnalysis/TestBase.cs`
  - Extends shared `TestBase`; exposes `ITestOrdersModule` and `ILabAnalysisModule`
  - Includes protected helper `SetupWorklistItemWithResultAsync(...)` — runs Group C chain; also calls `GenerateReportCommand` on `LabAnalysisModule`; returns `(WorklistItemId, OrderItemId)`

- [ ] **Task 6.2** — `WorklistItemCompletedFlowTests`
  - **Creates:** `LabAnalysis/WorklistItemCompletedFlowTests.cs`
  - **Test:** `WorklistItemCompleted_CompletesExamInTestOrders`
    1. `SetupWorklistItemWithResultAsync(...)` → `(WorklistItemId, OrderItemId)`
    2. Execute `CompleteWorklistItemCommand` on `LabAnalysisModule`
    3. `AssertEventually(new GetExamCompletedFromTestOrdersProbe(OrderItemId, TestOrdersModule), 15_000)`

---

### Phase 7: End-to-End

- [ ] **Task 7.1** — Full workflow test
  - **Creates:** `FullWorkflowTests.cs` (at root of `IntegrationEvents/`)
  - **Test:** `FullLaboratoryWorkflow_FromOrderAcceptanceToCompletion`
    1. Place order, request exam, accept exam (TestOrders)
    2. Mark patient arrived, create barcode, record collection (SampleCollection)
    3. Probe: exam InProgress (TestOrders), AnalyzerSample created, WorklistItem created, WorklistItemId assigned
    4. Forward raw result (Analyzer)
    5. Probe: analysis result recorded (LabAnalysis)
    6. Generate report, complete worklist item (LabAnalysis)
    7. Probe: exam Completed (TestOrders)
  - Use 60 000 ms timeout for each probe

---

## Summary

| Phase | Focus | Task Count |
|---|---|---|
| Resolve Open Design Decisions | Prerequisites | 4 |
| Probes | Observable state queries | 7 |
| Group A — TestOrders → SampleCollection | 2 tests | 3 |
| Group B — SampleCollection fan-out | 4 tests | 5 |
| Group C — Analyzer → LabAnalysis | 1 test | 2 |
| Group D — LabAnalysis → TestOrders | 1 test | 2 |
| End-to-End | Full workflow | 1 |
| **Total** | | **24** |
