# Sample Collection Workflow — Implementation Tasks

**Status:** In Progress
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
- [ ] `Application/Collections/CreateCollectionRequest/PatientArrivedNotificationHandler.cs` — publishes `PatientArrivedIntegrationEvent` via `IEventsBus`
- [x] `Application/Collections/MovePatientToWaiting/PatientWaitingNotification.cs` *(co-located with command)*
- [x] `Application/Collections/CallPatient/PatientCalledNotification.cs` *(co-located with command)*
- [x] `Application/Collections/CreateBarcode/BarcodeCreatedNotification.cs` *(co-located with command)*
- [ ] `Application/Collections/BarcodeCreated/BarcodeCreatedNotificationHandler.cs` — publishes `BarcodeCreatedIntegrationEvent` via `IEventsBus`
- [x] `Application/Collections/RecordSampleCollection/SampleCollectedNotification.cs` *(co-located with command)*
- [ ] `Application/Collections/RecordSampleCollection/SampleCollectedNotificationHandler.cs` — publishes `SampleCollectedIntegrationEvent` via `IEventsBus`
- [ ] `Application/Collections/SampleCreatedForExam/SampleCreatedForExamNotification.cs`
- [ ] `Application/Collections/ExamAddedToExistingSample/ExamAddedToExistingSampleNotification.cs`

---

## Layer 3: Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable properties, JSON-serializable.

- [ ] `IntegrationEvents/PatientArrivedIntegrationEvent.cs`
  - Properties: `CollectionRequestId` (`Guid`), `PatientId` (`Guid`)
- [ ] `IntegrationEvents/BarcodeCreatedIntegrationEvent.cs`
  - Properties: `CollectionRequestId` (`Guid`), `SampleId` (`Guid`), `Barcode` (`string`)
- [ ] `IntegrationEvents/SampleCollectedIntegrationEvent.cs`
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

- [ ] `Infrastructure/Configurations/SampleCollectionStartup.cs`
  - Populate BiMap with all 7 domain event → notification type mappings (5/7 done: `PatientArrivedNotification`, `PatientWaitingNotification`, `PatientCalledNotification`, `BarcodeCreatedNotification`, `SampleCollectedNotification`):

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

- [ ] `ISampleCollectionModule.cs` — public interface exposing one method per command:
  - `Task<Guid> CreateCollectionRequestAsync(Guid patientId, Guid orderId)`
  - `Task AddExamToCollectionAsync(Guid collectionRequestId, Guid examId)`
  - `Task MovePatientToWaitingAsync(Guid collectionRequestId)`
  - `Task CallPatientAsync(Guid collectionRequestId)`
  - `Task CreateBarcodeAsync(Guid collectionRequestId, Guid sampleId)`
  - `Task RecordSampleCollectionAsync(Guid collectionRequestId, Guid sampleId)`
- [ ] `SampleCollectionModule.cs` — implements `ISampleCollectionModule`, dispatches via `IMediator`

---

## Layer 5: Integration Tests

Location: `Tests/IntegrationTests/Collections/`
Pattern: `TestBase.ExecuteCommandAsync(command)` → `AssertOutboxMessage<TNotification>()`

- [ ] `Tests/IntegrationTests/Collections/CollectionRequestTests.cs`

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
- [ ] `dotnet test` IntegrationTests — all 6 new integration tests pass

### Analyzer rules to watch

- `CA1002`: expose `IReadOnlyCollection<T>`, not `List<T>`
- `CA1307`: string comparisons need `StringComparison` overload
- `CA2007`: `.ConfigureAwait(false)` on all awaited tasks
- `CA1707`: no underscores in public/test method names (use `PascalCase`)
- `CA1716`: avoid reserved keywords in namespaces
