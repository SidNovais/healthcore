# Implementation Tasks: PatientManagement Module

**Tech Spec:** [docs/specs/PatientManagement-TechSpec.md](./PatientManagement-TechSpec.md)
**Date:** 2026-05-25

---

## Prerequisites

- `PatientManagement.IntegrationEvents` NuGet/project reference must be added to `TestOrders.Application` before Phase 9 tasks can be executed.
- No cross-module event enrichment required — PatientManagement is a new, independent publisher.

---

## Task List

### Phase 1: Module Skeleton

- [x] **Task 1.1** — Scaffold module structure
  - **Skill:** `/create-module PatientManagement`
  - **Creates:** 7 projects (Domain, Application, Infrastructure, IntegrationEvents, UnitTests, IntegrationTests, ArchTests) + 4 database migrations (schema, inbox, internal commands, outbox)
  - **Verify:** `dotnet build` succeeds

---

### Phase 2: Domain Layer (TDD) (COMPLETED)

- [x] **Task 2.1** — Write failing unit tests for `Patient` registration
  - **Skill:** `/unit-test PatientManagement register a patient`
  - **Creates:** `Tests/UnitTests/Patients/PatientTests.cs`, `PatientFactory.cs`, `PatientSampleData.cs`
  - **Tests:** `RegisterPatientIsSuccessful`
  - **Expected:** Tests fail — `PatientRegisteredDomainEvent` and `Patient.Register()` do not exist yet

- [x] **Task 2.2** — Implement `Patient` aggregate with `Register` method
  - **Skill:** `/domain PatientManagement register a patient`
  - **Creates:** `Domain/Patients/Patient.cs`, `PatientId.cs`, `PatientStatus.cs`, `PatientInfo.cs`, `Events/PatientRegisteredDomainEvent.cs`
  - **Verify:** Unit tests from Task 2.1 pass

- [x] **Task 2.3** — Write failing unit tests for `Patient.Update`
  - **Skill:** `/unit-test PatientManagement update a patient`
  - **Modifies:** `Tests/UnitTests/Patients/PatientTests.cs`
  - **Tests:** `UpdatePatientIsSuccessful`, `UpdatePatientThrowsWhenAnonymized`
  - **Expected:** Tests fail — `PatientUpdatedDomainEvent`, `CannotUpdateAnonymizedPatientRule`, and `Patient.Update()` do not exist yet

- [x] **Task 2.4** — Implement `Patient.Update` method
  - **Skill:** `/domain PatientManagement update a patient`
  - **Creates:** `Events/PatientUpdatedDomainEvent.cs`, `Rules/CannotUpdateAnonymizedPatientRule.cs`
  - **Modifies:** `Domain/Patients/Patient.cs`
  - **Verify:** Unit tests from Task 2.3 pass

- [x] **Task 2.5** — Write failing unit tests for `Patient.Anonymize`
  - **Skill:** `/unit-test PatientManagement anonymize a patient`
  - **Modifies:** `Tests/UnitTests/Patients/PatientTests.cs`
  - **Tests:** `AnonymizePatientIsSuccessful`, `AnonymizePatientThrowsWhenAlreadyAnonymized`
  - **Expected:** Tests fail — `PatientAnonymizedDomainEvent`, `CannotAnonymizeAlreadyAnonymizedPatientRule`, and `Patient.Anonymize()` do not exist yet

- [x] **Task 2.6** — Implement `Patient.Anonymize` method
  - **Skill:** `/domain PatientManagement anonymize a patient`
  - **Creates:** `Events/PatientAnonymizedDomainEvent.cs`, `Rules/CannotAnonymizeAlreadyAnonymizedPatientRule.cs`
  - **Modifies:** `Domain/Patients/Patient.cs`, `Domain/Patients/PatientInfo.cs` (add `Anonymized()` factory)
  - **Verify:** Unit tests from Task 2.5 pass

---

### Phase 3: Application Layer — Commands & Notifications (COMPLETED)

- [x] **Task 3.1** — Implement `RegisterPatientCommand` and handler
  - **Skill:** `/application PatientManagement RegisterPatient command`
  - **Creates:** `Application/Patients/RegisterPatient/RegisterPatientCommand.cs`, `RegisterPatientCommandHandler.cs`
  - **Pattern:** Handler calls `IAggregateStore.Start(patient)` — extends `CommandBase<Guid>`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.2** — Implement `PatientRegisteredNotification` and projection
  - **Skill:** `/application PatientManagement PatientRegistered notification`
  - **Creates:** `Application/Patients/RegisterPatient/PatientRegisteredNotification.cs`, `PatientRegisteredNotificationProjection.cs`
  - **Note:** Projection performs INSERT into `PatientDetails` with Status = `"Active"`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.3** — Implement `UpdatePatientCommand` and handler
  - **Skill:** `/application PatientManagement UpdatePatient command`
  - **Creates:** `Application/Patients/UpdatePatient/UpdatePatientCommand.cs`, `UpdatePatientCommandHandler.cs`
  - **Pattern:** Handler calls `IAggregateStore.AppendChanges(patient)` — extends `CommandBase`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.4** — Implement `PatientUpdatedNotification` and projection
  - **Skill:** `/application PatientManagement PatientUpdated notification`
  - **Creates:** `Application/Patients/UpdatePatient/PatientUpdatedNotification.cs`, `PatientUpdatedNotificationProjection.cs`
  - **Note:** Projection performs UPDATE on all PII fields
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.5** — Implement `AnonymizePatientCommand` and handler
  - **Skill:** `/application PatientManagement AnonymizePatient command`
  - **Creates:** `Application/Patients/AnonymizePatient/AnonymizePatientCommand.cs`, `AnonymizePatientCommandHandler.cs`
  - **Pattern:** Handler calls `IAggregateStore.AppendChanges(patient)` — extends `CommandBase`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.6** — Implement `PatientAnonymizedNotification` and projection
  - **Skill:** `/application PatientManagement PatientAnonymized notification`
  - **Creates:** `Application/Patients/AnonymizePatient/PatientAnonymizedNotification.cs`, `PatientAnonymizedNotificationProjection.cs`
  - **Note:** Projection performs UPDATE — sets Status = `"Anonymized"`, replaces PII fields with sentinel values (`"ANONYMIZED"` / null / `1900-01-01`), sets AnonymizedAt
  - **Verify:** `dotnet build` succeeds

---

### Phase 4: Application Layer — Read Models

- [x] **Task 4.1** — Implement `PatientDetails` read model
  - **Skill:** `/application PatientManagement GetPatientDetails read model`
  - **Creates:** `Application/Patients/GetPatientDetails/PatientDetailsDto.cs`, `GetPatientDetailsQuery.cs`, `GetPatientDetailsQueryHandler.cs`, `PatientDetailsProjector.cs`
  - **Note:** Projector has `When()` for all 3 domain events + fall-through `When(IDomainEvent)`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 4.2** — Implement `SearchPatients` read model query
  - **Manual**
  - **Creates:** `Application/Patients/SearchPatients/PatientSearchResultDto.cs`, `SearchPatientsQuery.cs`, `SearchPatientsQueryHandler.cs`
  - **Note:** Handler uses `WHERE "FullName" ILIKE @SearchTerm OR "DocumentId" ILIKE @SearchTerm`; `SearchTerm` should be passed as `%{term}%` by the query
  - **Verify:** `dotnet build` succeeds

---

### Phase 5: Integration Events

- [x] **Task 5.1** — Define outbound integration events
  - **Manual**
  - **Creates:** `IntegrationEvents/PatientRegisteredIntegrationEvent.cs`, `PatientUpdatedIntegrationEvent.cs`, `PatientAnonymizedIntegrationEvent.cs`
  - **Note:** All inherit `IntegrationEvent(Guid id, DateTime occurredAt)`; nullable fields use `string?`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 5.2** — Implement publish notification handlers
  - **Skill:** `/application PatientManagement publish event notification handlers`
  - **Creates:** `Application/Patients/RegisterPatient/PatientRegisteredPublishEventNotificationHandler.cs`, `Application/Patients/UpdatePatient/PatientUpdatedPublishEventNotificationHandler.cs`, `Application/Patients/AnonymizePatient/PatientAnonymizedPublishEventNotificationHandler.cs`
  - **Note:** Use `notification.DomainEvent.{Prop}` inline — no intermediate variable
  - **Verify:** `dotnet build` succeeds

---

### Phase 6: Infrastructure Wiring

- [x] **Task 6.1** — Register domain events in `DomainEventTypeMappings`
  - **Manual**
  - **Modifies:** `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`
  - **Adds:** `AddEventType<PatientRegisteredDomainEvent>()`, `AddEventType<PatientUpdatedDomainEvent>()`, `AddEventType<PatientAnonymizedDomainEvent>()`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 6.2** — Register notifications in OutboxModule BiMap
  - **Manual**
  - **Modifies:** `Infrastructure/Configurations/PatientManagementStartup.cs`
  - **Adds:** 3 BiMap entries for `PatientRegisteredNotification`, `PatientUpdatedNotification`, `PatientAnonymizedNotification`
  - **Verify:** `dotnet build` succeeds

---

### Phase 7: Database Migrations

- [x] **Task 7.1** — Create `PatientDetails` table migration
  - **Session note (2026-05-30):** Also added schema, InboxMessages, InternalCommands, OutboxMessages migrations (not generated by `/create-module`) and wired PatientManagement.Infrastructure into the Database runner.
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Database/PatientManagement/{timestamp}_PatientManagementModule_AddTablePatientDetails.cs`
  - **Columns:** `Id (UUID PK)`, `FullName (VARCHAR 255)`, `DateOfBirth (TIMESTAMPTZ)`, `Gender (VARCHAR 50 NULL)`, `MothersFullName (VARCHAR 255 NULL)`, `DocumentId (VARCHAR 100 NULL)`, `Phone (VARCHAR 50 NULL)`, `Email (VARCHAR 255 NULL)`, `Status (VARCHAR 50)`, `RegisteredAt (TIMESTAMPTZ)`, `AnonymizedAt (TIMESTAMPTZ NULL)`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

---

### Phase 8: Integration Tests (TDD)

- [ ] **Task 8.1** — Write integration tests for `RegisterPatient`
  - **Skill:** `/integration-test PatientManagement register a patient`
  - **Creates:** `Tests/IntegrationTests/Patients/PatientTests.cs`, `GetPatientDetailsFromPatientManagementProbe.cs`, `PatientFactory.cs`, `PatientSampleData.cs`
  - **Tests:** `RegisterPatientIsSuccessful` — asserts `PatientDetails.Status == "Active"`, all fields set correctly

- [ ] **Task 8.2** — Write integration tests for `UpdatePatient`
  - **Skill:** `/integration-test PatientManagement update a patient`
  - **Modifies:** `Tests/IntegrationTests/Patients/PatientTests.cs`
  - **Tests:** `UpdatePatientIsSuccessful` — asserts PII fields updated in `PatientDetails`

- [ ] **Task 8.3** — Write integration tests for `AnonymizePatient`
  - **Skill:** `/integration-test PatientManagement anonymize a patient`
  - **Modifies:** `Tests/IntegrationTests/Patients/PatientTests.cs`
  - **Tests:** `AnonymizePatientIsSuccessful` — asserts `Status == "Anonymized"`, `FullName == "ANONYMIZED"`, `DocumentId == "ANONYMIZED"`, `AnonymizedAt` set

- [ ] **Task 8.4** — Verify all integration tests pass
  - **Verify:** `dotnet test src/HC.LIS/HC.LIS.Modules/PatientManagement/Tests/IntegrationTests/HC.LIS.Modules.PatientManagement.IntegrationTests.csproj` — all tests green

---

### Phase 9: Cross-Module Changes (TestOrders)

- [ ] **Task 9.1** — Add `PatientManagement.IntegrationEvents` project reference to TestOrders Application
  - **Manual**
  - **Modifies:** `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/HC.LIS.Modules.TestOrders.Application.csproj`
  - **Adds:** `<ProjectReference>` to `HC.LIS.Modules.PatientManagement.IntegrationEvents.csproj`
  - **Verify:** `dotnet build` succeeds

- [ ] **Task 9.2** — Define `IPatientSnapshotRepository` interface in TestOrders Application
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Patients/IPatientSnapshotRepository.cs`
  - **Methods:** `StoreAsync(...)`, `UpdateAsync(...)`, `AnonymizeAsync(...)`

- [ ] **Task 9.3** — Implement 3 internal commands + handlers in TestOrders Application
  - **Manual**
  - **Creates:**
    - `Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommand.cs` + `StorePatientSnapshotByPatientIdCommandHandler.cs`
    - `Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommand.cs` + `UpdatePatientSnapshotByPatientIdCommandHandler.cs`
    - `Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommand.cs` + `AnonymizePatientSnapshotByPatientIdCommandHandler.cs`
  - **Note:** Commands extend `InternalCommandBase`, use `[method: JsonConstructor]`; handlers inject `IPatientSnapshotRepository`
  - **Verify:** `dotnet build` succeeds

- [ ] **Task 9.4** — Implement 3 integration event handlers in TestOrders Application
  - **Manual**
  - **Creates:**
    - `Application/Patients/StorePatientSnapshot/PatientRegisteredIntegrationEventHandler.cs` (class: `PatientRegisteredIntegrationEventNotificationHandler`)
    - `Application/Patients/UpdatePatientSnapshot/PatientUpdatedIntegrationEventHandler.cs` (class: `PatientUpdatedIntegrationEventNotificationHandler`)
    - `Application/Patients/AnonymizePatientSnapshot/PatientAnonymizedIntegrationEventHandler.cs` (class: `PatientAnonymizedIntegrationEventNotificationHandler`)
  - **Note:** Each handler injects `ICommandsScheduler` and calls `EnqueueAsync(internalCommand)` — never executes the command directly
  - **Verify:** `dotnet build` succeeds

- [ ] **Task 9.5** — Implement `IPatientSnapshotRepository` in TestOrders Infrastructure
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Modules/TestOrders/Infrastructure/Patients/PatientSnapshotRepository.cs`
  - **Note:** Uses Dapper + `ISqlConnectionFactory`; INSERT on store, UPDATE on update/anonymize; all awaited calls use `.ConfigureAwait(false)`
  - **Verify:** `dotnet build` succeeds

- [ ] **Task 9.6** — Implement `GetPatientSnapshotDetailsQuery` and handler in TestOrders
  - **Manual**
  - **Creates:** `Application/Patients/GetPatientSnapshotDetails/PatientSnapshotDetailsDto.cs`, `GetPatientSnapshotDetailsQuery.cs`, `GetPatientSnapshotDetailsQueryHandler.cs`
  - **Note:** Dapper SELECT from `test_orders.PatientSnapshotDetails`
  - **Verify:** `dotnet build` succeeds

- [ ] **Task 9.7** — Register internal commands in TestOrders BiMap + EventsBus subscriptions
  - **Manual**
  - **Modifies:** `Infrastructure/Configurations/TestOrdersStartup.cs` — add 3 BiMap entries for `StorePatientSnapshotByPatientIdCommand`, `UpdatePatientSnapshotByPatientIdCommand`, `AnonymizePatientSnapshotByPatientIdCommand`
  - **Modifies:** `Infrastructure/Configurations/EventsBus/EventsBusModule.cs` (or `EventsBusStartup.cs`) — subscribe to `PatientRegisteredIntegrationEvent`, `PatientUpdatedIntegrationEvent`, `PatientAnonymizedIntegrationEvent`
  - **Verify:** `dotnet build` succeeds

- [ ] **Task 9.8** — Create `PatientSnapshotDetails` table migration in TestOrders
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Database/TestOrders/{timestamp}_TestOrdersModule_AddTablePatientSnapshotDetails.cs`
  - **Columns:** `Id (UUID PK)`, `FullName (VARCHAR 255)`, `DateOfBirth (TIMESTAMPTZ)`, `Gender (VARCHAR 50 NULL)`, `MothersFullName (VARCHAR 255 NULL)`, `DocumentId (VARCHAR 100 NULL)`, `Phone (VARCHAR 50 NULL)`, `Email (VARCHAR 255 NULL)`, `Status (VARCHAR 50)`, `RegisteredAt (TIMESTAMPTZ)`, `AnonymizedAt (TIMESTAMPTZ NULL)`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

---

## Summary

| Phase | Task Count | Complexity |
|---|---|---|
| Phase 1: Module Skeleton | 1 | Low |
| Phase 2: Domain (TDD) | 6 | Medium-High |
| Phase 3: Application — Commands & Notifications | 6 | Medium |
| Phase 4: Application — Read Models | 2 | Medium |
| Phase 5: Integration Events | 2 | Medium |
| Phase 6: Infrastructure Wiring | 2 | Low |
| Phase 7: Database Migration | 1 | Low |
| Phase 8: Integration Tests | 4 | Medium |
| Phase 9: Cross-Module (TestOrders) | 8 | Medium-High |
| **Total** | **32** | |
