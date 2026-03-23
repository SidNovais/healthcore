# Sample Collection Workflow — Implementation Tasks

**Status:** Complete
**Domain layer:** Complete (aggregate, events, rules, unit tests)
**Remaining:** Application, IntegrationEvents, Infrastructure, Integration Tests

> **Convention:** After finishing each step, update this file — check off completed items and add a session note with any implementation details worth remembering.

### Session notes — 2026-03-20

- `CreateCollectionRequestCommand` + handler implemented (Step 1 complete)
- `PatientArrivedNotification` added — co-located in `CreateCollectionRequest/` folder (same pattern as TestOrders, not a separate `PatientArrived/` folder)
- `DomainEventTypeMappings` populated with all 7 domain events
- `SampleCollectionStartup` BiMap wired for `PatientArrivedNotification` only (remaining notifications to be added as each step is implemented)
- `CollectionRequest.Create` takes plain `Guid` params, **not** typed IDs — task descriptions above that say `new PatientId(...)` are inaccurate; just pass `command.PatientId` directly
- Build: 0 warnings, 0 errors. Unit tests: 14 passed.
- `AddExamToCollectionCommand` + handler implemented (Step 2 complete)
- `AddExamToCollectionCommand` has `TubeType (string)` in addition to `CollectionRequestId` and `ExamId` — the task description omitted it but the domain `AddExam(Guid examId, string tubeType)` requires it
- Load pattern uses `CollectionRequestId` typed ID: `_aggregateStore.Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))` — same pattern needed for all remaining handlers
- Save pattern uses `AppendChanges`: `_aggregateStore.AppendChanges(request)` — not `Append`
- Build: 0 warnings, 0 errors.
- `MovePatientToWaitingCommand` carries `WaitingAt (DateTime)` in addition to `CollectionRequestId` — the task description omitted it but `MoveToWaiting(DateTime waitingAt)` requires it
- `PatientWaitingNotification` co-located in `MovePatientToWaiting/` folder; BiMap key is `"PatientWaitingNotification"` (full class name, matching the existing `"PatientArrivedNotification"` pattern)
- Build: 0 warnings, 0 errors. Step 3 complete.
- `CallPatientCommand` carries `TechnicianId (Guid)` and `CalledAt (DateTime)` in addition to `CollectionRequestId` — the task description omitted them but `CallPatient(Guid technicianId, DateTime calledAt)` requires both
- `PatientCalledNotification` co-located in `CallPatient/` folder; BiMap key is `"PatientCalledNotification"`
- Build: 0 warnings, 0 errors. Unit tests: 14 passed. Step 4 complete.
- `CreateBarcodeCommand` carries `TubeType (string)`, `BarcodeValue (string)`, `TechnicianId (Guid)`, `CreatedAt (DateTime)` in addition to `CollectionRequestId` — the task description omitted them but `CreateBarcode(string tubeType, string barcodeValue, Guid technicianId, DateTime createdAt)` requires all four
- `BarcodeCreatedNotification` co-located in `CreateBarcode/` folder; BiMap key is `"BarcodeCreatedNotification"` (4th entry, now 4/7 wired)
- Build: 0 warnings, 0 errors. Unit tests: 14 passed. Step 5 complete.
- `RecordSampleCollectionCommand` carries `SampleId (Guid)`, `TechnicianId (Guid)`, `CollectedAt (DateTime)` in addition to `CollectionRequestId` — task description omitted them but `RecordCollection(Guid sampleId, Guid technicianId, DateTime collectedAt)` requires all three
- `SampleCollectedNotification` co-located in `RecordSampleCollection/` folder; BiMap key is `"SampleCollectedNotification"` (5th entry, now 5/7 wired)
- Build: 0 warnings, 0 errors. Unit tests: 14 passed. Step 6 complete.

### Session notes — 2026-03-22

- Module facade already existed using generic dispatcher pattern (same as TestOrders) — task checkboxes were stale; no code changes needed
- Integration tests: `CollectionRequestTests.cs` created with 6 `[Fact]` methods — each self-contained (xUnit creates new instance per test, DB cleared in constructor)
- `CreateBarcode` requires `Waiting` status; `RecordCollection` requires `Called` status — so barcode must be created before calling patient in the full workflow test
- `RecordSampleCollectionIsSuccessful` reads `SampleCreatedForExamNotification` outbox message to retrieve the aggregate-generated `SampleId` before proceeding
- Test method names use PascalCase (no underscores) to comply with CA1707
- In `[Fact]` test methods use `.ConfigureAwait(true)` (xUnit1030 forbids `false` in test methods); private helper uses `.ConfigureAwait(false)` (CA2007)
- Build: 0 warnings, 0 errors.

### Session notes — 2026-03-21

- `SampleCreatedForExamNotification` and `ExamAddedToExistingSampleNotification` co-located in `AddExamToCollection/` folder — both domain events fire from the same `AddExam()` domain method depending on whether a sample already exists for the tube type
- BiMap now 7/7: added `"SampleCreatedForExamNotification"` and `"ExamAddedToExistingSampleNotification"` entries to `SampleCollectionStartup`
- Build: 0 warnings, 0 errors. Unit tests: 14 passed.
- Integration events added: `PatientArrivedIntegrationEvent`, `BarcodeCreatedIntegrationEvent`, `SampleCollectedIntegrationEvent` — all in `IntegrationEvents/` project, inherit `IntegrationEvent(id, occurredAt)` from `HC.COre.Infrastructure.EventBus` (note typo in namespace — matches rest of project)
- Publish handlers added: `PatientArrivedPublishEventNotificationHandler`, `BarcodeCreatedPublishEventNotificationHandler`, `SampleCollectedPublishEventNotificationHandler` — each implements `INotificationHandler<T>`, injects `IEventsBus`, constructs event from `notification.DomainEvent`, uses `notification.Id` as event id
- `BarcodeCreated` maps `domainEvent.BarcodeValue` → integration event `Barcode` property (task spec uses `Barcode` not `BarcodeValue`)
- Build: 0 warnings, 0 errors.

---

## Layer 1: Application — Commands & Handlers

One subfolder per step under `Application/Collections/`.

### Step 1 — CreateCollectionRequest

- [x] `Application/Collections/CreateCollectionRequest/CreateCollectionRequestCommand.cs`
  - Properties: `CollectionRequestId`, `PatientId`, `OrderId`, `ExamPreparationVerified`, `ArrivedAt`
  - Extends `CommandBase<Guid>`
- [x] `Application/Collections/CreateCollectionRequest/CreateCollectionRequestCommandHandler.cs`
  - Calls `CollectionRequest.Create(command.CollectionRequestId, command.PatientId, command.OrderId, ...)`
  - Persists via `IAggregateStore.Start()`
  - Returns `command.CollectionRequestId`

### Step 2 — AddExamToCollection

- [x] `Application/Collections/AddExamToCollection/AddExamToCollectionCommand.cs`
  - Properties: `CollectionRequestId`, `ExamId` (both `Guid`), `TubeType (string)`
  - Extends `CommandBase`
- [x] `Application/Collections/AddExamToCollection/AddExamToCollectionCommandHandler.cs`
  - Loads aggregate, calls `CollectionRequest.AddExam(command.ExamId, command.TubeType)`
  - Appends via `AppendChanges`

### Step 3 — MovePatientToWaiting

- [x] `Application/Collections/MovePatientToWaiting/MovePatientToWaitingCommand.cs`
  - Properties: `CollectionRequestId` (`Guid`), `WaitingAt` (`DateTime`)
  - Extends `CommandBase`
- [x] `Application/Collections/MovePatientToWaiting/MovePatientToWaitingCommandHandler.cs`
  - Loads aggregate, calls `CollectionRequest.MoveToWaiting(command.WaitingAt)`
  - Appends via `AppendChanges`

### Step 4 — CallPatient

- [x] `Application/Collections/CallPatient/CallPatientCommand.cs`
  - Property: `CollectionRequestId` (`Guid`)
  - Implements `ICommand`
- [x] `Application/Collections/CallPatient/CallPatientCommandHandler.cs`
  - Loads aggregate, calls `CollectionRequest.CallPatient()`
  - Saves aggregate

### Step 5 — CreateBarcode

- [x] `Application/Collections/CreateBarcode/CreateBarcodeCommand.cs`
  - Properties: `CollectionRequestId`, `SampleId` (both `Guid`)
  - Implements `ICommand`
- [x] `Application/Collections/CreateBarcode/CreateBarcodeCommandHandler.cs`
  - Loads aggregate, calls `CollectionRequest.CreateBarcode(command.SampleId)`
  - Saves aggregate

### Step 6 — RecordSampleCollection

- [x] `Application/Collections/RecordSampleCollection/RecordSampleCollectionCommand.cs`
  - Properties: `CollectionRequestId`, `SampleId` (both `Guid`)
  - Implements `ICommand`
- [x] `Application/Collections/RecordSampleCollection/RecordSampleCollectionCommandHandler.cs`
  - Loads aggregate, calls `CollectionRequest.RecordCollection(command.SampleId)`
  - Saves aggregate

---

## Layer 2: Application — Notifications

One notification class per domain event. Notifications with an integration event also need a handler.

| Notification | From Domain Event | Integration Event? |
|---|---|---|
| `PatientArrivedNotification` | `PatientArrivedDomainEvent` | **Yes** |
| `PatientWaitingNotification` | `PatientWaitingDomainEvent` | No |
| `PatientCalledNotification` | `PatientCalledDomainEvent` | No |
| `BarcodeCreatedNotification` | `BarcodeCreatedDomainEvent` | **Yes** |
| `SampleCollectedNotification` | `SampleCollectedDomainEvent` | **Yes** |
| `SampleCreatedForExamNotification` | `SampleCreatedForExamDomainEvent` | No |
| `ExamAddedToExistingSampleNotification` | `ExamAddedToExistingSampleDomainEvent` | No |

### Notification files

- [x] `Application/Collections/CreateCollectionRequest/PatientArrivedNotification.cs` *(co-located with command, not in a separate folder)*
- [x] `Application/Collections/CreateCollectionRequest/PatientArrivedPublishEventNotificationHandler.cs` — publishes `PatientArrivedIntegrationEvent` via `IEventsBus`
- [x] `Application/Collections/MovePatientToWaiting/PatientWaitingNotification.cs` *(co-located with command)*
- [x] `Application/Collections/CallPatient/PatientCalledNotification.cs` *(co-located with command)*
- [x] `Application/Collections/CreateBarcode/BarcodeCreatedNotification.cs` *(co-located with command)*
- [x] `Application/Collections/CreateBarcode/BarcodeCreatedPublishEventNotificationHandler.cs` — publishes `BarcodeCreatedIntegrationEvent` via `IEventsBus`
- [x] `Application/Collections/RecordSampleCollection/SampleCollectedNotification.cs` *(co-located with command)*
- [x] `Application/Collections/RecordSampleCollection/SampleCollectedPublishEventNotificationHandler.cs` — publishes `SampleCollectedIntegrationEvent` via `IEventsBus`
- [x] `Application/Collections/AddExamToCollection/SampleCreatedForExamNotification.cs`
- [x] `Application/Collections/AddExamToCollection/ExamAddedToExistingSampleNotification.cs`

---

## Layer 3: Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable properties, JSON-serializable.

- [x] `IntegrationEvents/PatientArrivedIntegrationEvent.cs`
  - Properties: `CollectionRequestId` (`Guid`), `PatientId` (`Guid`)
- [x] `IntegrationEvents/BarcodeCreatedIntegrationEvent.cs`
  - Properties: `CollectionRequestId` (`Guid`), `SampleId` (`Guid`), `Barcode` (`string`)
- [x] `IntegrationEvents/SampleCollectedIntegrationEvent.cs`
  - Properties: `CollectionRequestId` (`Guid`), `SampleId` (`Guid`)

---

## Layer 4: Infrastructure Wiring

### DomainEventTypeMappings

- [x] `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`
  - All 7 domain events registered

```csharp
// Pattern example (adapt names):
options.Events.AddEventType<PatientArrivedDomainEvent>();
options.Events.AddEventType<PatientWaitingDomainEvent>();
options.Events.AddEventType<PatientCalledDomainEvent>();
options.Events.AddEventType<BarcodeCreatedDomainEvent>();
options.Events.AddEventType<SampleCollectedDomainEvent>();
options.Events.AddEventType<SampleCreatedForExamDomainEvent>();
options.Events.AddEventType<ExamAddedToExistingSampleDomainEvent>();
```

### SampleCollectionStartup — OutboxModule BiMap

- [x] `Infrastructure/Configurations/SampleCollectionStartup.cs`
  - Populate BiMap with all 7 domain event → notification type mappings (7/7 done):

```csharp
// Pattern (adapt to actual notification class names):
notificationsBiMap.Add("PatientArrived",      typeof(PatientArrivedNotification));
notificationsBiMap.Add("PatientWaiting",      typeof(PatientWaitingNotification));
notificationsBiMap.Add("PatientCalled",       typeof(PatientCalledNotification));
notificationsBiMap.Add("BarcodeCreated",      typeof(BarcodeCreatedNotification));
notificationsBiMap.Add("SampleCollected",     typeof(SampleCollectedNotification));
notificationsBiMap.Add("SampleCreatedForExam",         typeof(SampleCreatedForExamNotification));
notificationsBiMap.Add("ExamAddedToExistingSample",    typeof(ExamAddedToExistingSampleNotification));
```

### Module Facade

- [x] `ISampleCollectionModule.cs` — generic dispatcher interface (`ExecuteCommandAsync`, `ExecuteQueryAsync`) — same pattern as `ITestOrdersModule`, already implemented
- [x] `SampleCollectionModule.cs` — implements `ISampleCollectionModule`, dispatches commands via `CommandsExecutor` and queries via `SampleCollectionCompositionRoot` + `IMediator`

---

## Layer 5: Integration Tests

Location: `Tests/IntegrationTests/Collections/`
Pattern: `TestBase.ExecuteCommandAsync(command)` → `AssertOutboxMessage<TNotification>()`

- [x] `Tests/IntegrationTests/Collections/CollectionRequestTests.cs`

| Test Method | Command Sent | Outbox Assertion |
|---|---|---|
| `CreateCollectionRequest_IsSuccessful` | `CreateCollectionRequestCommand` | `AssertOutboxMessage<PatientArrivedNotification>()` |
| `AddExamToCollection_IsSuccessful` | `AddExamToCollectionCommand` | `AssertOutboxMessage<SampleCreatedForExamNotification>()` |
| `MovePatientToWaiting_IsSuccessful` | `MovePatientToWaitingCommand` | `AssertOutboxMessage<PatientWaitingNotification>()` |
| `CallPatient_IsSuccessful` | `CallPatientCommand` | `AssertOutboxMessage<PatientCalledNotification>()` |
| `CreateBarcode_IsSuccessful` | `CreateBarcodeCommand` | `AssertOutboxMessage<BarcodeCreatedNotification>()` |
| `RecordSampleCollection_IsSuccessful` | `RecordSampleCollectionCommand` | `AssertOutboxMessage<SampleCollectedNotification>()` |

---

## Reference Files

| Purpose | Path |
|---|---|
| Command + Handler pattern | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/*/` |
| Notification + Handler pattern | `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/*/` |
| Integration event pattern | `src/HC.LIS/HC.LIS.Modules/TestOrders/IntegrationEvents/` |
| Startup / BiMap wiring | `src/HC.LIS/HC.LIS.Modules/TestOrders/Infrastructure/Configurations/TestOrdersStartup.cs` |
| DomainEventTypeMappings | `src/HC.LIS/HC.LIS.Modules/TestOrders/Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs` |
| Module facade | `src/HC.LIS/HC.LIS.Modules/TestOrders/TestOrdersModule.cs` |
| Integration test pattern | `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/` |
| **Target startup** | `src/HC.LIS/HC.LIS.Modules/SampleCollection/Infrastructure/Configurations/SampleCollectionStartup.cs` |
| **Target event mappings** | `src/HC.LIS/HC.LIS.Modules/SampleCollection/Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs` |
| **Target integration events** | `src/HC.LIS/HC.LIS.Modules/SampleCollection/IntegrationEvents/` |

---

## Verification Checklist

- [x] `dotnet build` — zero warnings (`TreatWarningsAsErrors=true`)
- [x] `dotnet test` UnitTests — 14 tests pass
- [x] `dotnet test` IntegrationTests — all 6 new integration tests pass

### Analyzer rules to watch

- `CA1002`: expose `IReadOnlyCollection<T>`, not `List<T>`
- `CA1307`: string comparisons need `StringComparison` overload
- `CA2007`: `.ConfigureAwait(false)` on all awaited tasks
- `CA1707`: no underscores in public/test method names (use `PascalCase`)
- `CA1716`: avoid reserved keywords in namespaces

---

## Layer 6: Projections & Queries

Two read models, each with a projector, notification projections, query handler, DTO, and integration tests.
Pattern: identical to `TestOrders` (`OrderDetailsProjector`, `OrderItemDetailsProjector`).

### Read Model 1 — CollectionRequestDetails

Tracks the overall collection request lifecycle.

| Column | Type | Populated by |
|---|---|---|
| `Id` | GUID PK | `PatientArrivedDomainEvent` |
| `PatientId` | GUID | `PatientArrivedDomainEvent` |
| `OrderId` | GUID | `PatientArrivedDomainEvent` |
| `Status` | VARCHAR(50) | All status transitions |
| `ArrivedAt` | TIMESTAMPTZ | `PatientArrivedDomainEvent` |
| `WaitingAt` | TIMESTAMPTZ NULL | `PatientWaitingDomainEvent` |
| `CalledAt` | TIMESTAMPTZ NULL | `PatientCalledDomainEvent` |

#### Application files
- [x] `Application/Collections/GetCollectionRequestDetails/CollectionRequestDetailsDto.cs`
- [x] `Application/Collections/GetCollectionRequestDetails/GetCollectionRequestDetailsQuery.cs`
- [x] `Application/Collections/GetCollectionRequestDetails/GetCollectionRequestDetailsQueryHandler.cs` — Dapper SELECT
- [x] `Application/Collections/GetCollectionRequestDetails/CollectionRequestDetailsProjector.cs`
  - `When(PatientArrivedDomainEvent)` → INSERT
  - `When(PatientWaitingDomainEvent)` → UPDATE Status, WaitingAt
  - `When(PatientCalledDomainEvent)` → UPDATE Status, CalledAt
  - `When(IDomainEvent)` → fall-through (no-op)
- [x] `Application/Collections/CreateCollectionRequest/PatientArrivedNotificationProjection.cs` — co-located with command *(not in GetCollectionRequestDetails/)*
- [x] `Application/Collections/MovePatientToWaiting/PatientWaitingNotificationProjection.cs` — co-located with command
- [x] `Application/Collections/CallPatient/PatientCalledNotificationProjection.cs` — co-located with command

### Read Model 2 — SampleDetails

Tracks individual sample lifecycle.

| Column | Type | Populated by |
|---|---|---|
| `Id` | GUID PK | `SampleCreatedForExamDomainEvent` |
| `CollectionRequestId` | GUID | `SampleCreatedForExamDomainEvent` |
| `TubeType` | VARCHAR(255) | `SampleCreatedForExamDomainEvent` |
| `Barcode` | VARCHAR(255) NULL | `BarcodeCreatedDomainEvent` |
| `Status` | VARCHAR(50) | `SampleCreated`, `BarcodeCreated`, `SampleCollected` |
| `CollectedAt` | TIMESTAMPTZ NULL | `SampleCollectedDomainEvent` |

> Note: `ExamAddedToExistingSampleDomainEvent` does not alter SampleDetails (no exam tracking needed in this read model).

#### Application files
- [x] `Application/Collections/GetSampleDetails/SampleDetailsDto.cs`
- [x] `Application/Collections/GetSampleDetails/GetSampleDetailsQuery.cs`
- [x] `Application/Collections/GetSampleDetails/GetSampleDetailsQueryHandler.cs` — Dapper SELECT
- [x] `Application/Collections/GetSampleDetails/SampleDetailsProjector.cs`
  - `When(SampleCreatedForExamDomainEvent)` → INSERT (Status = "Pending")
  - `When(BarcodeCreatedDomainEvent)` → UPDATE Barcode, Status = "BarcodeCreated"
  - `When(SampleCollectedDomainEvent)` → UPDATE Status = "Collected", CollectedAt
  - `When(IDomainEvent)` → fall-through (no-op)
- [x] `Application/Collections/AddExamToCollection/SampleCreatedForExamNotificationProjection.cs` — co-located with command *(not in GetSampleDetails/)*
- [x] `Application/Collections/CreateBarcode/BarcodeCreatedNotificationProjection.cs` — co-located with command (second handler alongside publish handler)
- [x] `Application/Collections/RecordSampleCollection/SampleCollectedNotificationProjection.cs` — co-located with command (second handler alongside publish handler)

### Database Migrations

- [x] `src/HC.LIS/HC.LIS.Database/SampleCollection/20260322185000_SampleCollectionModule_AddTableCollectionRequestDetails.cs`
- [x] `src/HC.LIS/HC.LIS.Database/SampleCollection/20260322185100_SampleCollectionModule_AddTableSampleDetails.cs`

### Integration Tests

- [x] `Tests/IntegrationTests/Collections/GetCollectionRequestDetailsFromSampleCollectionProbe.cs` — `IProbe<CollectionRequestDetailsDto>`
- [x] `Tests/IntegrationTests/Collections/GetSampleDetailsFromSampleCollectionProbe.cs` — `IProbe<SampleDetailsDto>`
- [x] `Tests/IntegrationTests/TestBase.cs` — `ClearDatabase()` now truncates `CollectionRequestDetails` and `SampleDetails`; added `GetEventually()` method and `IDisposable` (calls `SampleCollectionStartup.Stop()`)
- [x] `Tests/IntegrationTests/Collections/CollectionRequestTests.cs` — all 6 tests refactored to assert on projected read model state via `GetEventually(probe, 15000)`

| Test | After Command | Assertion |
|---|---|---|
| `CreateCollectionRequestIsSuccessful` | `CreateCollectionRequestCommand` | `CollectionRequestDetails.Status == "Arrived"`, `PatientId`, `OrderId`, `ArrivedAt` |
| `MovePatientToWaitingIsSuccessful` | `MovePatientToWaitingCommand` | `Status == "Waiting"`, `WaitingAt` set |
| `CallPatientIsSuccessful` | `CallPatientCommand` | `Status == "Called"`, `CalledAt` set |
| `AddExamToCollectionIsSuccessful` | `AddExamToCollectionCommand` | `SampleDetails.Status == "Pending"`, `TubeType`, `CollectionRequestId` |
| `CreateBarcodeIsSuccessful` | `CreateBarcodeCommand` | `SampleDetails.Barcode` set, `Status == "BarcodeCreated"` |
| `RecordSampleCollectionIsSuccessful` | `RecordSampleCollectionCommand` | `Status == "Collected"`, `CollectedAt` set |

### Verification

- [x] `dotnet build` — zero warnings
- [x] `dotnet test` UnitTests — 14 tests pass (no domain changes)
- [x] `dotnet test` IntegrationTests — 6/6 passed (2026-03-23)

### Session notes — 2026-03-22

- Notification projection classes co-located with their command folder (same pattern as TestOrders), **not** inside the read model folder — e.g. `PatientArrivedNotificationProjection` lives in `CreateCollectionRequest/`, not `GetCollectionRequestDetails/`
- Projectors (`CollectionRequestDetailsProjector`, `SampleDetailsProjector`) are auto-registered by `DataAccessModule` assembly scan — no manual DI wiring needed
- `TestBase` now implements `IDisposable` and calls `SampleCollectionStartup.Stop()` on disposal
- `GetEventually()` added as a `public static` method on `TestBase`, identical signature to TestOrders
- `AddExamToCollection`, `CreateBarcode`, and `RecordSampleCollection` tests keep one `GetLastOutboxMessage<SampleCreatedForExamNotification>()` call to retrieve the aggregate-generated `SampleId`, then assert via projection
- Build: 0 warnings, 0 errors. Unit tests: 14 passed.
