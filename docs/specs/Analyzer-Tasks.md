# Implementation Tasks: Analyzer Module

**Tech Spec:** [docs/specs/Analyzer-TechSpec.md](./Analyzer-TechSpec.md)
**Date:** 2026-04-09

> **Workflow rule:** After implementing each task, mark it as done (`- [x]`) in this file before moving on.

---

## Prerequisites

1. **Enrich `SampleCollectedIntegrationEvent`** in SampleCollection module with `PatientName (string)`, `PatientBirthdate (DateTime)`, `PatientGender (string)` — required for Analyzer to construct HL7 responses (see Phase 9, Task 9.1).
2. **`WorklistItemCreatedIntegrationEvent`** from LabAnalysis already exists and carries `WorklistItemId`, `SampleBarcode`, `ExamCode` — no enrichment needed.

---

## Task List

### Phase 1: Module Skeleton

> Skip — `src/HC.LIS/HC.LIS.Modules/Analyzer/` already exists with full Infrastructure scaffold.

---

### Phase 2: Domain Layer (TDD)

- [x] **Task 2.1** — Write failing unit tests for `AnalyzerSample` creation
  - **Skill:** `/unit-test Analyzer create an AnalyzerSample`
  - **Creates:** `Tests/UnitTests/AnalyzerSamples/AnalyzerSampleTests.cs`, `AnalyzerSampleFactory.cs`, `AnalyzerSampleSampleData.cs`
  - **Tests:** `CreateAnalyzerSampleIsSuccessful`
  - **Expected:** Tests fail — `AnalyzerSampleCreatedDomainEvent` and `AnalyzerSample.Create()` do not exist yet

- [x] **Task 2.2** — Implement `AnalyzerSample` aggregate with `Create` method
  - **Skill:** `/domain Analyzer create an AnalyzerSample`
  - **Creates:** `Domain/AnalyzerSamples/AnalyzerSample.cs`, `AnalyzerSampleId.cs`, `AnalyzerSampleStatus.cs`, `AnalyzerSampleExam.cs`, `Events/AnalyzerSampleCreatedDomainEvent.cs`
  - **Verify:** Unit tests from Task 2.1 pass

- [x] **Task 2.3** — Write failing unit tests for `AssignWorklistItem`
  - **Skill:** `/unit-test Analyzer assign a worklist item to an AnalyzerSample exam`
  - **Modifies:** `Tests/UnitTests/AnalyzerSamples/AnalyzerSampleTests.cs`
  - **Tests:** `AssignWorklistItemIsSuccessful`, `AssignWorklistItemShouldBrokeExamMustExistInSampleRuleWhenExamDoesNotExist`
  - **Expected:** Tests fail — `WorklistItemAssignedDomainEvent`, `ExamMustExistInSampleRule`, and `AnalyzerSample.AssignWorklistItem()` do not exist yet

- [x] **Task 2.4** — Implement `AssignWorklistItem` on `AnalyzerSample`
  - **Skill:** `/domain Analyzer assign a worklist item to an AnalyzerSample exam`
  - **Creates:** `Domain/AnalyzerSamples/Events/WorklistItemAssignedDomainEvent.cs`, `Domain/AnalyzerSamples/Rules/ExamMustExistInSampleRule.cs`
  - **Modifies:** `Domain/AnalyzerSamples/AnalyzerSample.cs`
  - **Verify:** Unit tests from Task 2.3 pass

- [x] **Task 2.5** — Write failing unit tests for `DispatchInfo`
  - **Skill:** `/unit-test Analyzer dispatch sample info for an AnalyzerSample`
  - **Modifies:** `Tests/UnitTests/AnalyzerSamples/AnalyzerSampleTests.cs`
  - **Tests:** `DispatchInfoIsSuccessful`, `DispatchInfoShouldBrokeCannotDispatchInfoForNonAwaitingQuerySampleRuleWhenNotAwaitingQuery`
  - **Expected:** Tests fail — `SampleInfoDispatchedDomainEvent`, `CannotDispatchInfoForNonAwaitingQuerySampleRule`, and `AnalyzerSample.DispatchInfo()` do not exist yet

- [x] **Task 2.6** — Implement `DispatchInfo` on `AnalyzerSample`
  - **Skill:** `/domain Analyzer dispatch sample info for an AnalyzerSample`
  - **Creates:** `Domain/AnalyzerSamples/Events/SampleInfoDispatchedDomainEvent.cs`, `Domain/AnalyzerSamples/Rules/CannotDispatchInfoForNonAwaitingQuerySampleRule.cs`
  - **Modifies:** `Domain/AnalyzerSamples/AnalyzerSample.cs`
  - **Verify:** Unit tests from Task 2.5 pass

- [x] **Task 2.7** — Write failing unit tests for `ReceiveResult`
  - **Skill:** `/unit-test Analyzer receive an exam result for an AnalyzerSample`
  - **Modifies:** `Tests/UnitTests/AnalyzerSamples/AnalyzerSampleTests.cs`
  - **Tests:** `ReceiveExamResultIsSuccessful`, `ReceiveExamResultShouldBrokeCannotReceiveResultForNonDispatchedSampleRuleWhenNotDispatched`, `ReceiveExamResultShouldBrokeExamMustExistInSampleRuleWhenExamDoesNotExist`, `ReceiveLastExamResultSetsAllResultsReceivedTrue`
  - **Expected:** Tests fail — `ExamResultReceivedDomainEvent`, `CannotReceiveResultForNonDispatchedSampleRule`, and `AnalyzerSample.ReceiveResult()` do not exist yet

- [x] **Task 2.8** — Implement `ReceiveResult` on `AnalyzerSample`
  - **Skill:** `/domain Analyzer receive an exam result for an AnalyzerSample`
  - **Creates:** `Domain/AnalyzerSamples/Events/ExamResultReceivedDomainEvent.cs`, `Domain/AnalyzerSamples/Rules/CannotReceiveResultForNonDispatchedSampleRule.cs`
  - **Modifies:** `Domain/AnalyzerSamples/AnalyzerSample.cs`
  - **Verify:** Unit tests from Task 2.7 pass

---

### Phase 3: Application Layer — Commands & Handlers

- [ ] **Task 3.1** — Implement `CreateAnalyzerSampleCommand` and handler
  - **Creates:** `Application/AnalyzerSamples/CreateAnalyzerSample/CreateAnalyzerSampleCommand.cs`, `CreateAnalyzerSampleCommandHandler.cs`, `ExamInfoDto.cs`
  - **Pattern:** Handler uses `IAggregateStore.Start()` for creation

- [ ] **Task 3.2** — Implement `AnalyzerSampleCreatedNotification` and projection
  - **Creates:** `Application/AnalyzerSamples/CreateAnalyzerSample/AnalyzerSampleCreatedNotification.cs`, `AnalyzerSampleCreatedNotificationProjection.cs`

- [ ] **Task 3.3** — Implement `AssignWorklistItemCommand` and handler
  - **Creates:** `Application/AnalyzerSamples/AssignWorklistItem/AssignWorklistItemCommand.cs`, `AssignWorklistItemCommandHandler.cs`
  - **Pattern:** Handler uses `IAggregateStore.Load()` + `AppendChanges`

- [ ] **Task 3.4** — Implement `WorklistItemAssignedNotification` and projection
  - **Creates:** `Application/AnalyzerSamples/AssignWorklistItem/WorklistItemAssignedNotification.cs`, `WorklistItemAssignedNotificationProjection.cs`

- [ ] **Task 3.5** — Implement `DispatchSampleInfoCommand` and handler
  - **Creates:** `Application/AnalyzerSamples/DispatchSampleInfo/DispatchSampleInfoCommand.cs`, `DispatchSampleInfoCommandHandler.cs`
  - **Pattern:** Handler uses `IAggregateStore.Load()` + `AppendChanges`

- [ ] **Task 3.6** — Implement `SampleInfoDispatchedNotification` and projection
  - **Creates:** `Application/AnalyzerSamples/DispatchSampleInfo/SampleInfoDispatchedNotification.cs`, `SampleInfoDispatchedNotificationProjection.cs`

- [ ] **Task 3.7** — Implement `ReceiveExamResultCommand` and handler
  - **Creates:** `Application/AnalyzerSamples/ReceiveExamResult/ReceiveExamResultCommand.cs`, `ReceiveExamResultCommandHandler.cs`
  - **Pattern:** Handler uses `IAggregateStore.Load()` + `AppendChanges`

- [ ] **Task 3.8** — Implement `ExamResultReceivedNotification` and projection
  - **Creates:** `Application/AnalyzerSamples/ReceiveExamResult/ExamResultReceivedNotification.cs`, `ExamResultReceivedNotificationProjection.cs`

---

### Phase 4: Application Layer — Read Model

- [ ] **Task 4.1** — Implement `AnalyzerSampleDetails` read model (DTO, query, handler, projector)
  - **Creates:** `Application/AnalyzerSamples/GetAnalyzerSampleDetails/AnalyzerSampleDetailsDto.cs`, `GetAnalyzerSampleDetailsQuery.cs`, `GetAnalyzerSampleDetailsQueryHandler.cs`, `AnalyzerSampleDetailsProjector.cs`

- [ ] **Task 4.2** — Implement `AnalyzerSampleExamDetails` read model (DTO, query, handler, projector)
  - **Creates:** `Application/AnalyzerSamples/GetAnalyzerSampleExamDetails/AnalyzerSampleExamDetailsDto.cs`, `GetAnalyzerSampleExamDetailsQuery.cs`, `GetAnalyzerSampleExamDetailsQueryHandler.cs`, `AnalyzerSampleExamDetailsProjector.cs`

- [ ] **Task 4.3** — Implement `GetSampleInfoByBarcode` query (JOIN of both read models)
  - **Creates:** `Application/AnalyzerSamples/GetSampleInfoByBarcode/SampleInfoDto.cs`, `GetSampleInfoByBarcodeQuery.cs`, `GetSampleInfoByBarcodeQueryHandler.cs`

- [ ] **Task 4.4** — Implement domain provider `IAnalyzerSampleByBarcodeProvider`
  - **Creates:** `Domain/AnalyzerSamples/IAnalyzerSampleByBarcodeProvider.cs`
  - **Creates:** `Infrastructure/DataAccess/AnalyzerSampleByBarcodeProvider.cs`

---

### Phase 5: Integration Events

- [ ] **Task 5.1** — Define outbound integration event `ExamResultReceivedIntegrationEvent`
  - **Creates:** `IntegrationEvents/ExamResultReceivedIntegrationEvent.cs`

- [ ] **Task 5.2** — Implement `ExamResultReceivedPublishEventNotificationHandler`
  - **Creates:** `Application/AnalyzerSamples/ReceiveExamResult/ExamResultReceivedPublishEventNotificationHandler.cs`

- [ ] **Task 5.3** — Implement internal command `CreateAnalyzerSampleBySampleCollectedCommand` and handler
  - **Creates:** `Application/AnalyzerSamples/CreateAnalyzerSample/CreateAnalyzerSampleBySampleCollectedCommand.cs`, `CreateAnalyzerSampleBySampleCollectedCommandHandler.cs`
  - **Pattern:** Extends `InternalCommandBase`; uses `[method: JsonConstructor]`; handler calls `AnalyzerSample.Create()` via `IAggregateStore.Start()`

- [ ] **Task 5.4** — Implement inbound handler for `SampleCollectedIntegrationEvent`
  - **Creates:** `Application/AnalyzerSamples/CreateAnalyzerSample/SampleCollectedIntegrationEventHandler.cs` (class: `SampleCollectedIntegrationEventNotificationHandler`)
  - **Pattern:** Schedules `CreateAnalyzerSampleBySampleCollectedCommand` via `ICommandsScheduler.EnqueueAsync()`
  - **Dependencies:** `SampleCollectedIntegrationEvent` must be enriched (Task 9.1)

- [ ] **Task 5.5** — Implement internal command `AssignWorklistItemByBarcodeAndExamCodeCommand` and handler
  - **Creates:** `Application/AnalyzerSamples/AssignWorklistItem/AssignWorklistItemByBarcodeAndExamCodeCommand.cs`, `AssignWorklistItemByBarcodeAndExamCodeCommandHandler.cs`
  - **Pattern:** Extends `InternalCommandBase`; handler resolves `AnalyzerSampleId` via `IAnalyzerSampleByBarcodeProvider`, then calls `AssignWorklistItem()` via `AppendChanges`

- [ ] **Task 5.6** — Implement inbound handler for `WorklistItemCreatedIntegrationEvent`
  - **Creates:** `Application/AnalyzerSamples/AssignWorklistItem/WorklistItemCreatedIntegrationEventHandler.cs` (class: `WorklistItemCreatedIntegrationEventNotificationHandler`)
  - **Pattern:** Schedules `AssignWorklistItemByBarcodeAndExamCodeCommand` via `ICommandsScheduler.EnqueueAsync()`
  - **Dependencies:** `IAnalyzerSampleByBarcodeProvider` (Task 4.4)

---

### Phase 6: Infrastructure Wiring

- [ ] **Task 6.1** — Register domain events in `DomainEventTypeMappings`
  - **Modifies:** `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`

- [ ] **Task 6.2** — Register notifications in OutboxModule BiMap
  - **Modifies:** `Infrastructure/Configurations/AnalyzerStartup.cs`

- [ ] **Task 6.3** — Register internal commands in InternalCommandsModule BiMap
  - **Modifies:** `Infrastructure/Configurations/AnalyzerStartup.cs`
  - **Registers:** `CreateAnalyzerSampleBySampleCollectedCommand`, `AssignWorklistItemByBarcodeAndExamCodeCommand`

- [ ] **Task 6.4** — Register EventsBus subscriptions
  - **Modifies:** `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs`

- [ ] **Task 6.5** — Register HL7 infrastructure services
  - **Creates:** `Infrastructure/HL7/HL7SampleInfoPresenter.cs`, `Infrastructure/HL7/HL7ResultParser.cs`
  - **Modifies:** `Infrastructure/Configurations/AnalyzerStartup.cs` (or new `HL7Module.cs`)

- [ ] **Task 6.6** — Register domain provider in DataAccessModule
  - **Modifies:** `Infrastructure/Configurations/DataAccess/DataAccessModule.cs`

---

### Phase 7: Database Migrations

- [ ] **Task 7.1** — Create `AnalyzerSampleDetails` table migration
  - **Creates:** `src/HC.LIS/HC.LIS.Database/Analyzer/20260409120400_AnalyzerModule_AddTableAnalyzerSampleDetails.cs`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

- [ ] **Task 7.2** — Create `AnalyzerSampleExamDetails` table migration
  - **Creates:** `src/HC.LIS/HC.LIS.Database/Analyzer/20260409120500_AnalyzerModule_AddTableAnalyzerSampleExamDetails.cs`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

---

### Phase 8: Integration Tests (TDD)

- [ ] **Task 8.1** — Write integration tests for `CreateAnalyzerSample`
  - **Skill:** `/integration-test Analyzer create an AnalyzerSample`
  - **Creates:** `Tests/IntegrationTests/AnalyzerSamples/AnalyzerSampleTests.cs`, `GetAnalyzerSampleDetailsFromAnalyzerProbe.cs`, `GetAnalyzerSampleExamDetailsFromAnalyzerProbe.cs`, `AnalyzerSampleFactory.cs`, `AnalyzerSampleSampleData.cs`
  - **Tests:** `CreateAnalyzerSampleIsSuccessful`

- [ ] **Task 8.2** — Write integration tests for `AssignWorklistItem`
  - **Skill:** `/integration-test Analyzer assign a worklist item to an AnalyzerSample exam`
  - **Modifies:** `Tests/IntegrationTests/AnalyzerSamples/AnalyzerSampleTests.cs`
  - **Tests:** `AssignWorklistItemIsSuccessful`

- [ ] **Task 8.3** — Write integration tests for `DispatchSampleInfo`
  - **Skill:** `/integration-test Analyzer dispatch sample info for an AnalyzerSample`
  - **Modifies:** `Tests/IntegrationTests/AnalyzerSamples/AnalyzerSampleTests.cs`
  - **Tests:** `DispatchSampleInfoIsSuccessful`

- [ ] **Task 8.4** — Write integration tests for `ReceiveExamResult`
  - **Skill:** `/integration-test Analyzer receive an exam result for an AnalyzerSample`
  - **Modifies:** `Tests/IntegrationTests/AnalyzerSamples/AnalyzerSampleTests.cs`
  - **Tests:** `ReceiveExamResultIsSuccessful`, `ReceiveAllExamResultsCompletesAnalyzerSample`

- [ ] **Task 8.5** — Verify all integration tests pass
  - **Verify:** `dotnet test src/HC.LIS/HC.LIS.Modules/Analyzer/Tests/IntegrationTests/HC.LIS.Modules.Analyzer.IntegrationTests.csproj`

---

### Phase 9: Cross-Module Changes

- [ ] **Task 9.1** — Enrich `SampleCollectedIntegrationEvent` in SampleCollection
  - **Modifies:** `src/HC.LIS/HC.LIS.Modules/SampleCollection/IntegrationEvents/SampleCollectedIntegrationEvent.cs`
  - **New fields:** `PatientName (string)`, `PatientBirthdate (DateTime)`, `PatientGender (string)`
  - **Also update:** The domain event and publish notification handler in SampleCollection that populates these fields

- [ ] **Task 9.2** — Add urgency support to TestOrders (deferred)
  - **Scope:** Requires `Order` aggregate change in TestOrders to support `IsUrgent` flag, propagation through `OrderItemRequestedIntegrationEvent` → SampleCollection → `SampleCollectedIntegrationEvent`
  - **Note:** This is a multi-module feature; defer to a separate task/PRD

---

## Summary

| Phase | Task Count | Complexity |
|---|---|---|
| Module Skeleton | 0 (skip) | — |
| Domain (TDD) | 8 | Medium-High |
| Application — Commands | 8 | Medium |
| Application — Read Model | 4 | Medium |
| Integration Events | 6 | Medium |
| Infrastructure Wiring | 6 | Low |
| Database Migrations | 2 | Low |
| Integration Tests | 5 | Medium |
| Cross-Module | 2 | Medium-High |
| **Total** | **41** | |
