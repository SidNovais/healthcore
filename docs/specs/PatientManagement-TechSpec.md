# Technical Spec: PatientManagement Module

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-05-25
**PRD Reference:** [docs/prd/PatientManagement.md](../prd/PatientManagement.md)

---

## 1. Overview

The PatientManagement module provides a patient registry for the HealthCore LIS platform. Receptionists register new patients and search for existing ones before placing orders, ensuring every order is tied to a verified patient identity. Patient data is managed with full event sourcing for auditability. Three integration events propagate patient data changes (registration, update, anonymization) to downstream modules — primarily TestOrders — so order workflows never require a synchronous cross-module call.

**Aggregate root:** `Patient`
**Schema:** `patient_management`

---

## 2. Aggregate: `Patient`

### 2.1 Identity

`PatientId` — typed ID wrapping `Guid`, string-keyed in Marten (`StreamIdentity.AsString`).

### 2.2 State Machine

```
Active → Anonymized
```

| Status | Meaning |
|---|---|
| `Active` | Patient is registered; PII is live and searchable |
| `Anonymized` | PII fields replaced with sentinel values; record retained for audit; no further mutations allowed |

### 2.3 Domain Methods & Events

| Method | Business Rule(s) | Domain Event Emitted |
|---|---|---|
| `Register(Guid patientId, string fullName, DateTime dateOfBirth, string? gender, string? mothersFullName, string? documentId, string? phone, string? email, DateTime registeredAt)` | — | `PatientRegisteredDomainEvent` |
| `Update(string fullName, DateTime dateOfBirth, string? gender, string? mothersFullName, string? documentId, string? phone, string? email, DateTime updatedAt)` | `CannotUpdateAnonymizedPatientRule` | `PatientUpdatedDomainEvent` |
| `Anonymize(DateTime anonymizedAt)` | `CannotAnonymizeAlreadyAnonymizedPatientRule` | `PatientAnonymizedDomainEvent` |

> **Implementation note:** The aggregate stores PII in a `PatientInfo` value object (`FullName`, `DateOfBirth`, `Gender`, `MothersFullName`, `DocumentId`, `Phone`, `Email`). Domain events carry primitives only — `PatientInfo` fields are unwrapped at call sites. `Anonymize()` replaces `_patientInfo` with `PatientInfo.Anonymized()` (sentinel values: `"ANONYMIZED"` for all string fields, `DateTime(1900, 1, 1)` for `DateOfBirth`). Role enforcement (IT Admin only) is the responsibility of the API layer.

### 2.4 Business Rules

| Class | Invariant |
|---|---|
| `CannotUpdateAnonymizedPatientRule` | Status must be `Active` to update patient data |
| `CannotAnonymizeAlreadyAnonymizedPatientRule` | Status must be `Active`; cannot anonymize a patient twice |

### 2.5 Domain Events (fields)

**`PatientRegisteredDomainEvent`**
- `PatientId` (Guid), `FullName` (string), `DateOfBirth` (DateTime), `Gender` (string?), `MothersFullName` (string?), `DocumentId` (string?), `Phone` (string?), `Email` (string?), `RegisteredAt` (DateTime)

**`PatientUpdatedDomainEvent`**
- `PatientId` (Guid), `FullName` (string), `DateOfBirth` (DateTime), `Gender` (string?), `MothersFullName` (string?), `DocumentId` (string?), `Phone` (string?), `Email` (string?), `UpdatedAt` (DateTime)

**`PatientAnonymizedDomainEvent`**
- `PatientId` (Guid), `AnonymizedAt` (DateTime)

---

## 3. Application Layer

### 3.1 Commands

Location: `Application/Patients/{CommandName}/`

| Command | Properties | Aggregate Method Called |
|---|---|---|
| `RegisterPatientCommand` | `PatientId (Guid)`, `FullName (string)`, `DateOfBirth (DateTime)`, `Gender (string?)`, `MothersFullName (string?)`, `DocumentId (string?)`, `Phone (string?)`, `Email (string?)`, `RegisteredAt (DateTime)` — extends `CommandBase<Guid>` | `Patient.Register(...)` via `IAggregateStore.Start()` |
| `UpdatePatientCommand` | `PatientId (Guid)`, `FullName (string)`, `DateOfBirth (DateTime)`, `Gender (string?)`, `MothersFullName (string?)`, `DocumentId (string?)`, `Phone (string?)`, `Email (string?)`, `UpdatedAt (DateTime)` — extends `CommandBase` | `Patient.Update(...)` via `AppendChanges` |
| `AnonymizePatientCommand` | `PatientId (Guid)`, `AnonymizedAt (DateTime)` — extends `CommandBase` | `Patient.Anonymize(...)` via `AppendChanges` |

### 3.2 Notifications

One notification per domain event; co-located in the same folder as the triggering command.

| Notification | Co-located With | Integration Event? |
|---|---|---|
| `PatientRegisteredNotification` | `RegisterPatient/` | **Yes** — emits `PatientRegisteredIntegrationEvent` |
| `PatientUpdatedNotification` | `UpdatePatient/` | **Yes** — emits `PatientUpdatedIntegrationEvent` |
| `PatientAnonymizedNotification` | `AnonymizePatient/` | **Yes** — emits `PatientAnonymizedIntegrationEvent` |

### 3.3 Notification Projection Handlers

Projections are co-located with their command folder (not in the read model folder).

| Projection Class | Co-located With | Read Model Updated |
|---|---|---|
| `PatientRegisteredNotificationProjection` | `RegisterPatient/` | `PatientDetails` — INSERT, Status = `"Active"` |
| `PatientUpdatedNotificationProjection` | `UpdatePatient/` | `PatientDetails` — UPDATE all PII fields |
| `PatientAnonymizedNotificationProjection` | `AnonymizePatient/` | `PatientDetails` — UPDATE Status = `"Anonymized"`, PII fields to sentinel values, AnonymizedAt |

---

## 4. Integration Events

### 4.1 Inbound — Subscriptions

None — PatientManagement does not subscribe to integration events from other modules.

### 4.2 Outbound — Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable, JSON-serializable.

**`PatientRegisteredIntegrationEvent`**
- `PatientId` (Guid)
- `FullName` (string)
- `DateOfBirth` (DateTime)
- `Gender` (string?) — nullable
- `MothersFullName` (string?) — nullable
- `DocumentId` (string?) — nullable
- `Phone` (string?) — nullable
- `Email` (string?) — nullable
- `RegisteredAt` (DateTime)

> Consumed by: TestOrders — triggers local patient snapshot creation.

**`PatientUpdatedIntegrationEvent`**
- `PatientId` (Guid)
- `FullName` (string)
- `DateOfBirth` (DateTime)
- `Gender` (string?) — nullable
- `MothersFullName` (string?) — nullable
- `DocumentId` (string?) — nullable
- `Phone` (string?) — nullable
- `Email` (string?) — nullable
- `UpdatedAt` (DateTime)

> Consumed by: TestOrders — triggers local patient snapshot update.

**`PatientAnonymizedIntegrationEvent`**
- `PatientId` (Guid)
- `AnonymizedAt` (DateTime)

> Consumed by: TestOrders (and all future modules holding patient snapshots) — triggers irreversible anonymization of local patient data. This action is permanent.

### 4.3 Cross-Module: TestOrders Integration

TestOrders must consume all three PatientManagement integration events and maintain a local `PatientSnapshotDetails` read model in the `test_orders` schema. This ensures order creation never requires a synchronous call to PatientManagement.

**Architectural pattern:** Integration event handler → `ICommandsScheduler.EnqueueAsync(internalCommand)` → internal command handler → `IPatientSnapshotRepository` (interface in `Application`, Dapper implementation in `Infrastructure`).

#### Integration event handlers (TestOrders Application layer)

File name convention: `{SourceEvent}IntegrationEventHandler.cs`; class name: `{SourceEvent}IntegrationEventNotificationHandler` (CA1711).

| Handler Class | Subscribes To | Internal Command Scheduled | File Location |
|---|---|---|---|
| `PatientRegisteredIntegrationEventNotificationHandler` | `PatientRegisteredIntegrationEvent` | `StorePatientSnapshotByPatientIdCommand` | `Application/Patients/StorePatientSnapshot/` |
| `PatientUpdatedIntegrationEventNotificationHandler` | `PatientUpdatedIntegrationEvent` | `UpdatePatientSnapshotByPatientIdCommand` | `Application/Patients/UpdatePatientSnapshot/` |
| `PatientAnonymizedIntegrationEventNotificationHandler` | `PatientAnonymizedIntegrationEvent` | `AnonymizePatientSnapshotByPatientIdCommand` | `Application/Patients/AnonymizePatientSnapshot/` |

#### Internal commands (TestOrders)

All extend `InternalCommandBase`, use `[method: JsonConstructor]` for deserialization, and must be registered in TestOrders' `InternalCommandsModule` BiMap.

| Internal Command | Properties |
|---|---|
| `StorePatientSnapshotByPatientIdCommand` | `PatientId (Guid)`, `FullName (string)`, `DateOfBirth (DateTime)`, `Gender (string?)`, `MothersFullName (string?)`, `DocumentId (string?)`, `Phone (string?)`, `Email (string?)`, `RegisteredAt (DateTime)` |
| `UpdatePatientSnapshotByPatientIdCommand` | `PatientId (Guid)`, `FullName (string)`, `DateOfBirth (DateTime)`, `Gender (string?)`, `MothersFullName (string?)`, `DocumentId (string?)`, `Phone (string?)`, `Email (string?)`, `UpdatedAt (DateTime)` |
| `AnonymizePatientSnapshotByPatientIdCommand` | `PatientId (Guid)`, `AnonymizedAt (DateTime)` |

#### Domain-defined provider interface (TestOrders)

Define `IPatientSnapshotRepository` in `TestOrders.Application/Patients/`:

```csharp
public interface IPatientSnapshotRepository
{
    Task StoreAsync(Guid patientId, string fullName, DateTime dateOfBirth,
        string? gender, string? mothersFullName, string? documentId,
        string? phone, string? email, DateTime registeredAt,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid patientId, string fullName, DateTime dateOfBirth,
        string? gender, string? mothersFullName, string? documentId,
        string? phone, string? email, DateTime updatedAt,
        CancellationToken cancellationToken = default);

    Task AnonymizeAsync(Guid patientId, DateTime anonymizedAt,
        CancellationToken cancellationToken = default);
}
```

Implemented in `TestOrders.Infrastructure` using Dapper against `test_orders.PatientSnapshotDetails`.

#### `PatientSnapshotDetails` read model (TestOrders)

Schema: `test_orders` | Table: `PatientSnapshotDetails`
Application files location: `Application/Patients/GetPatientSnapshotDetails/`

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `StorePatientSnapshotByPatientIdCommand` |
| `FullName` | VARCHAR(255) | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `DateOfBirth` | TIMESTAMPTZ | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `Gender` | VARCHAR(50) NULL | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `MothersFullName` | VARCHAR(255) NULL | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `DocumentId` | VARCHAR(100) NULL | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `Phone` | VARCHAR(50) NULL | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `Email` | VARCHAR(255) NULL | `StorePatientSnapshotByPatientIdCommand` / `UpdatePatientSnapshotByPatientIdCommand` |
| `Status` | VARCHAR(50) | All operations (`"Active"` / `"Anonymized"`) |
| `RegisteredAt` | TIMESTAMPTZ | `StorePatientSnapshotByPatientIdCommand` |
| `AnonymizedAt` | TIMESTAMPTZ NULL | `AnonymizePatientSnapshotByPatientIdCommand` |

Application files:
- `PatientSnapshotDetailsDto.cs`
- `GetPatientSnapshotDetailsQuery.cs` → `IQuery<PatientSnapshotDetailsDto?>`
- `GetPatientSnapshotDetailsQueryHandler.cs` — Dapper SELECT from `test_orders.PatientSnapshotDetails`

---

## 5. Read Models: PatientManagement

### 5.1 `PatientDetails`

Location: `Application/Patients/GetPatientDetails/`

#### Table Schema

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `PatientRegisteredDomainEvent` |
| `FullName` | VARCHAR(255) | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `DateOfBirth` | TIMESTAMPTZ | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `Gender` | VARCHAR(50) NULL | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `MothersFullName` | VARCHAR(255) NULL | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `DocumentId` | VARCHAR(100) NULL | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `Phone` | VARCHAR(50) NULL | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `Email` | VARCHAR(255) NULL | `PatientRegisteredDomainEvent` / `PatientUpdatedDomainEvent` / `PatientAnonymizedDomainEvent` |
| `Status` | VARCHAR(50) | All state transitions |
| `RegisteredAt` | TIMESTAMPTZ | `PatientRegisteredDomainEvent` |
| `AnonymizedAt` | TIMESTAMPTZ NULL | `PatientAnonymizedDomainEvent` |

#### Application Files

- `PatientDetailsDto.cs`
- `GetPatientDetailsQuery.cs` — `GetPatientDetailsQuery(Guid patientId)` → `IQuery<PatientDetailsDto?>`
- `GetPatientDetailsQueryHandler.cs` — Dapper SELECT from `patient_management.PatientDetails`
- `PatientDetailsProjector.cs`
  - `When(PatientRegisteredDomainEvent)` → INSERT, Status = `"Active"`
  - `When(PatientUpdatedDomainEvent)` → UPDATE all PII fields
  - `When(PatientAnonymizedDomainEvent)` → UPDATE Status = `"Anonymized"`, FullName = `"ANONYMIZED"`, DateOfBirth = `1900-01-01`, Gender = `null`, MothersFullName = `null`, DocumentId = `"ANONYMIZED"`, Phone = `null`, Email = `null`, AnonymizedAt
  - `When(IDomainEvent)` → fall-through (no-op)

### 5.2 `SearchPatients`

Location: `Application/Patients/SearchPatients/`

This query returns a filtered list of patients for receptionist lookup. It searches the `PatientDetails` table using PostgreSQL `ILIKE` for case-insensitive matching on `FullName` and `DocumentId`. Anonymized patients may appear in results with sentinel field values.

#### Application Files

- `PatientSearchResultDto.cs` — compact DTO: `Id (Guid)`, `FullName (string)`, `DateOfBirth (DateTime)`, `DocumentId (string?)`, `Status (string)`
- `SearchPatientsQuery.cs` — `SearchPatientsQuery(string searchTerm)` → `IQuery<IReadOnlyCollection<PatientSearchResultDto>>`
- `SearchPatientsQueryHandler.cs` — Dapper SELECT with `WHERE "FullName" ILIKE @SearchTerm OR "DocumentId" ILIKE @SearchTerm`

---

## 6. Infrastructure Wiring

### 6.1 DomainEventTypeMappings

Register all 3 domain events in `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`:

```csharp
options.Events.AddEventType<PatientRegisteredDomainEvent>();
options.Events.AddEventType<PatientUpdatedDomainEvent>();
options.Events.AddEventType<PatientAnonymizedDomainEvent>();
```

### 6.2 PatientManagementStartup — OutboxModule BiMap

Register all 3 notification type mappings in `Infrastructure/Configurations/PatientManagementStartup.cs`:

```csharp
notificationsBiMap.Add("PatientRegisteredNotification",  typeof(PatientRegisteredNotification));
notificationsBiMap.Add("PatientUpdatedNotification",     typeof(PatientUpdatedNotification));
notificationsBiMap.Add("PatientAnonymizedNotification",  typeof(PatientAnonymizedNotification));
```

### 6.3 PatientManagementStartup — InternalCommandsModule BiMap

No internal commands in PatientManagement — skip.

### 6.4 EventsBus Subscription

No inbound integration events in PatientManagement — skip.

### 6.5 Module Facade

`IPatientManagementModule` and `PatientManagementModule` — scaffolded by `/create-module`; generic dispatcher pattern, no changes needed.

### 6.6 TestOrders — InternalCommandsModule BiMap

Register the 3 new internal commands in `TestOrders` `InternalCommandsModule` BiMap:

```csharp
internalCommandsBiMap.Add("StorePatientSnapshotByPatientIdCommand",    typeof(StorePatientSnapshotByPatientIdCommand));
internalCommandsBiMap.Add("UpdatePatientSnapshotByPatientIdCommand",   typeof(UpdatePatientSnapshotByPatientIdCommand));
internalCommandsBiMap.Add("AnonymizePatientSnapshotByPatientIdCommand", typeof(AnonymizePatientSnapshotByPatientIdCommand));
```

### 6.7 TestOrders — EventsBus Subscriptions

Register the 3 integration event handlers in `TestOrders` `EventsBusModule`:

```csharp
// Subscribe to PatientManagement integration events
services.AddIntegrationEventHandler<PatientRegisteredIntegrationEvent, PatientRegisteredIntegrationEventNotificationHandler>();
services.AddIntegrationEventHandler<PatientUpdatedIntegrationEvent,    PatientUpdatedIntegrationEventNotificationHandler>();
services.AddIntegrationEventHandler<PatientAnonymizedIntegrationEvent,  PatientAnonymizedIntegrationEventNotificationHandler>();
```

---

## 7. Database Migrations

### 7.1 PatientManagement migrations

Location: `src/HC.LIS/HC.LIS.Database/PatientManagement/`

| File | Purpose |
|---|---|
| `{timestamp}_PatientManagementModule_AddSchemaPatientManagement.cs` | ⬜ Created by `/create-module` |
| `{timestamp}_PatientManagementModule_AddTableInboxMessages.cs` | ⬜ Created by `/create-module` |
| `{timestamp}_PatientManagementModule_AddTableInternalCommands.cs` | ⬜ Created by `/create-module` |
| `{timestamp}_PatientManagementModule_AddTableOutboxMessages.cs` | ⬜ Created by `/create-module` |
| `{timestamp}_PatientManagementModule_AddTablePatientDetails.cs` | ⬜ To be created |

### 7.2 TestOrders migrations

Location: `src/HC.LIS/HC.LIS.Database/TestOrders/`

| File | Purpose |
|---|---|
| `{timestamp}_TestOrdersModule_AddTablePatientSnapshotDetails.cs` | ⬜ To be created |

---

## 8. Unit Tests

Location: `Tests/UnitTests/Patients/PatientTests.cs`
Pattern: Arrange–Act–Assert, `AssertPublishedDomainEvent<T>()` on aggregate, FluentAssertions.

| Test | Asserts |
|---|---|
| `RegisterPatientIsSuccessful` | `PatientRegisteredDomainEvent` raised with all correct fields |
| `UpdatePatientIsSuccessful` | `PatientUpdatedDomainEvent` raised with updated fields |
| `AnonymizePatientIsSuccessful` | `PatientAnonymizedDomainEvent` raised with `PatientId` and `AnonymizedAt` |
| `UpdatePatientThrowsWhenAnonymized` | `BaseBusinessRuleException` with `CannotUpdateAnonymizedPatientRule` |
| `AnonymizePatientThrowsWhenAlreadyAnonymized` | `BaseBusinessRuleException` with `CannotAnonymizeAlreadyAnonymizedPatientRule` |

---

## 9. Integration Tests

Location: `Tests/IntegrationTests/Patients/`
Pattern: `TestBase.ExecuteCommandAsync(command)` → `GetEventually(probe, timeoutMs)` on projected read model.

| Test | Command Sent | Read Model Assertion |
|---|---|---|
| `RegisterPatientIsSuccessful` | `RegisterPatientCommand` | `PatientDetails.Status == "Active"`, all identity fields set, nullable fields correct |
| `UpdatePatientIsSuccessful` | `UpdatePatientCommand` | `PatientDetails` PII fields updated to new values |
| `AnonymizePatientIsSuccessful` | `AnonymizePatientCommand` | `PatientDetails.Status == "Anonymized"`, `FullName == "ANONYMIZED"`, `DocumentId == "ANONYMIZED"`, `AnonymizedAt` set |

---

## 10. Open Design Decisions

| # | Decision | Options | Recommendation |
|---|---|---|---|
| 1 | Anonymization role guard | API-layer enforcement vs. domain-layer policy object | **Resolved:** API layer only. IT Admin role validated in the endpoint; domain has no role awareness. |
| 2 | Document ID format validation | Free-text vs. country-specific format (CPF, SSN, etc.) | **Deferred:** Free-text this release. Format-specific validation added later without domain changes — validation belongs at the API/application boundary. |
| 3 | Duplicate detection | Enforce by name + DOB + documentId vs. no enforcement | **Deferred:** No enforcement in this release. Add as a separate `CheckDuplicatePatientRule` when deduplication requirements are clarified. |
| 4 | Event store PII after anonymization | Purge historical events vs. aggregate state override only | **Resolved:** Only current aggregate state is overwritten (`PatientInfo.Anonymized()`). Prior events in the Marten event store retain original PII. Storage-layer event stream purge (for HIPAA right-to-be-forgotten) is a separate infrastructure concern deferred to a future release. |
| 5 | `PatientUpdatedDomainEvent` — full replace vs. delta | Carry all 7 fields vs. carry only changed fields | **Resolved:** Full-state capture — all 7 PII fields in every update event. Simpler projection logic; delta events add complexity with no benefit at current scale. |
