# Technical Spec: Analyzer Module

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-04-09
**PRD Reference:** [docs/prd/Analyzer.md](../prd/Analyzer.md)

---

## 1. Overview

The Analyzer module automates bidirectional communication between the LIS and clinical analyzers using the HL7 protocol. It stores sample and patient information from upstream integration events, responds to analyzer barcode queries with HL7-formatted patient/sample data, and receives/persists analyzer results — forwarding them to the LabAnalysis module for worklist item completion.

**Aggregate root:** `AnalyzerSample`
**Schema:** `analyzer`

---

## 2. Aggregate: `AnalyzerSample`

### 2.1 Identity

`AnalyzerSampleId` — typed ID wrapping `Guid`, string-keyed in Marten (`StreamIdentity.AsString`).

### 2.2 State Machine

```
AwaitingQuery → InfoDispatched → ResultReceived
```

| Status | Meaning |
|---|---|
| `AwaitingQuery` | Sample info stored from integration events; waiting for analyzer to scan barcode |
| `InfoDispatched` | Patient/sample data sent to analyzer in HL7 response |
| `ResultReceived` | All exam results received from analyzer; forwarded to LabAnalysis |

### 2.3 Domain Methods & Events

| Method | Business Rule(s) | Domain Event Emitted |
|---|---|---|
| `Create(Guid analyzerSampleId, Guid sampleId, Guid patientId, string sampleBarcode, string patientName, DateTime patientBirthdate, string patientGender, IReadOnlyCollection<ExamInfo> exams, DateTime createdAt)` | — | `AnalyzerSampleCreatedDomainEvent` |
| `AssignWorklistItem(string examMnemonic, Guid worklistItemId, DateTime assignedAt)` | `ExamMustExistInSampleRule` | `WorklistItemAssignedDomainEvent` |
| `DispatchInfo(DateTime dispatchedAt)` | `CannotDispatchInfoForNonAwaitingQuerySampleRule` | `SampleInfoDispatchedDomainEvent` |
| `ReceiveResult(string examMnemonic, string resultValue, string resultUnit, string referenceRange, Guid instrumentId, DateTime recordedAt)` | `CannotReceiveResultForNonDispatchedSampleRule`, `ExamMustExistInSampleRule` | `ExamResultReceivedDomainEvent` |

> **Child entity:** `AnalyzerSampleExam` — each exam within the sample, holding `ExamMnemonic`, `WorklistItemId` (nullable until assigned), and `ResultReceived` flag. Created during `AnalyzerSample.Create()` from the `exams` collection.

> **Note on `Create` params:** `ExamInfo` is a simple record `(Guid ExamId, string ExamMnemonic)` from `SampleCollection.IntegrationEvents`. The aggregate constructor unwraps it to create `AnalyzerSampleExam` child entities. The domain event carries the exams as a serializable collection of primitives.

### 2.4 Business Rules

| Class | Invariant |
|---|---|
| `ExamMustExistInSampleRule` | The exam mnemonic must match one of the sample's registered exams |
| `CannotDispatchInfoForNonAwaitingQuerySampleRule` | Status must be `AwaitingQuery` to dispatch info |
| `CannotReceiveResultForNonDispatchedSampleRule` | Status must be `InfoDispatched` to receive results |

### 2.5 Domain Events (fields)

**`AnalyzerSampleCreatedDomainEvent`**
- `AnalyzerSampleId` (Guid), `SampleId` (Guid), `PatientId` (Guid), `SampleBarcode` (string), `PatientName` (string), `PatientBirthdate` (DateTime), `PatientGender` (string), `ExamMnemonics` (IReadOnlyCollection\<string\>), `CreatedAt` (DateTime)

**`WorklistItemAssignedDomainEvent`**
- `AnalyzerSampleId` (Guid), `ExamMnemonic` (string), `WorklistItemId` (Guid), `AssignedAt` (DateTime)

**`SampleInfoDispatchedDomainEvent`**
- `AnalyzerSampleId` (Guid), `SampleBarcode` (string), `DispatchedAt` (DateTime)

**`ExamResultReceivedDomainEvent`**
- `AnalyzerSampleId` (Guid), `ExamMnemonic` (string), `WorklistItemId` (Guid), `ResultValue` (string), `ResultUnit` (string), `ReferenceRange` (string), `InstrumentId` (Guid), `AllResultsReceived` (bool), `RecordedAt` (DateTime)

> `AllResultsReceived` is `true` when this is the last exam in the sample to receive a result. The `When()` handler uses this flag to transition sample status to `ResultReceived`.

---

## 3. Application Layer

### 3.1 Commands

Location: `Application/AnalyzerSamples/{CommandName}/`

| Command | Properties | Aggregate Method Called |
|---|---|---|
| `CreateAnalyzerSampleCommand` | `AnalyzerSampleId (Guid)`, `SampleId (Guid)`, `PatientId (Guid)`, `SampleBarcode (string)`, `PatientName (string)`, `PatientBirthdate (DateTime)`, `PatientGender (string)`, `Exams (IReadOnlyCollection<ExamInfoDto>)`, `CreatedAt (DateTime)` — extends `CommandBase<Guid>` | `AnalyzerSample.Create(...)` via `IAggregateStore.Start()` |
| `AssignWorklistItemCommand` | `AnalyzerSampleId (Guid)`, `ExamMnemonic (string)`, `WorklistItemId (Guid)`, `AssignedAt (DateTime)` — extends `CommandBase` | `AnalyzerSample.AssignWorklistItem(...)` via `AppendChanges` |

### 3.1b Internal Commands (scheduled from integration event handlers)

| Internal Command | Properties | Delegates To |
|---|---|---|
| `CreateAnalyzerSampleBySampleCollectedCommand` | `Id (Guid)`, `SampleId (Guid)`, `PatientId (Guid)`, `SampleBarcode (string)`, `PatientName (string)`, `PatientBirthdate (DateTime)`, `PatientGender (string)`, `Exams (IReadOnlyCollection<ExamInfoDto>)`, `CreatedAt (DateTime)` — extends `InternalCommandBase` | Handler creates `CreateAnalyzerSampleCommand` and executes it, or directly calls `AnalyzerSample.Create(...)` via `IAggregateStore.Start()` |
| `AssignWorklistItemByBarcodeAndExamCodeCommand` | `Id (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`, `WorklistItemId (Guid)`, `AssignedAt (DateTime)` — extends `InternalCommandBase` | Handler resolves `AnalyzerSampleId` via `IAnalyzerSampleByBarcodeProvider`, then calls `AnalyzerSample.AssignWorklistItem(...)` via `AppendChanges` |

> Internal commands use `[method: JsonConstructor]` attribute for deserialization from the `InternalCommands` table. The `Id` parameter is the internal command ID (not the aggregate ID).
| `DispatchSampleInfoCommand` | `AnalyzerSampleId (Guid)`, `DispatchedAt (DateTime)` — extends `CommandBase` | `AnalyzerSample.DispatchInfo(...)` via `AppendChanges` |
| `ReceiveExamResultCommand` | `AnalyzerSampleId (Guid)`, `ExamMnemonic (string)`, `ResultValue (string)`, `ResultUnit (string)`, `ReferenceRange (string)`, `InstrumentId (Guid)`, `RecordedAt (DateTime)` — extends `CommandBase` | `AnalyzerSample.ReceiveResult(...)` via `AppendChanges` |

> **`ExamInfoDto`** — `record ExamInfoDto(Guid ExamId, string ExamMnemonic)` defined in the command folder, used to decouple the command from `SampleCollection.IntegrationEvents.ExamInfo`.

> **Domain provider:** `IAnalyzerSampleByBarcodeProvider` — interface in Domain, implemented in Infrastructure (Dapper). Returns `AnalyzerSampleId?` for a given barcode. Used by `ReceiveExamResultCommand` handler when the ConsoleApp passes a barcode instead of the aggregate ID.

### 3.2 Notifications

| Notification | Co-located With | Integration Event? |
|---|---|---|
| `AnalyzerSampleCreatedNotification` | `CreateAnalyzerSample/` | No |
| `WorklistItemAssignedNotification` | `AssignWorklistItem/` | No |
| `SampleInfoDispatchedNotification` | `DispatchSampleInfo/` | No |
| `ExamResultReceivedNotification` | `ReceiveExamResult/` | **Yes** — emits `ExamResultReceivedIntegrationEvent` to LabAnalysis |

### 3.3 Notification Projection Handlers

| Projection Class | Co-located With | Read Model Updated |
|---|---|---|
| `AnalyzerSampleCreatedNotificationProjection` | `CreateAnalyzerSample/` | `AnalyzerSampleDetails` — INSERT |
| `WorklistItemAssignedNotificationProjection` | `AssignWorklistItem/` | `AnalyzerSampleExamDetails` — UPDATE WorklistItemId |
| `SampleInfoDispatchedNotificationProjection` | `DispatchSampleInfo/` | `AnalyzerSampleDetails` — UPDATE Status |
| `ExamResultReceivedNotificationProjection` | `ReceiveExamResult/` | `AnalyzerSampleExamDetails` — UPDATE ResultValue, ResultUnit, ReferenceRange; `AnalyzerSampleDetails` — UPDATE Status (if AllResultsReceived) |

### 3.4 HL7 Presentation Layer

| Component | Layer | Purpose |
|---|---|---|
| `ISampleInfoPresenter` | Application (interface) | Converts `SampleInfoDto` to a presentation format |
| `HL7SampleInfoPresenter` | Infrastructure | Serializes `SampleInfoDto` → HL7 v2.x message string |
| `IHL7ResultParser` | Application (interface) | Parses an HL7 result message into a structured DTO |
| `HL7ResultParser` | Infrastructure | Deserializes HL7 v2.x ORU message → `AnalyzerResultDto` |

> **Clean Architecture adapter pattern:** The ConsoleApp calls the module facade. For barcode queries, it executes `GetSampleInfoByBarcodeQuery` → gets `SampleInfoDto` → calls `ISampleInfoPresenter.Format()` to obtain the HL7 string. For inbound results, the ConsoleApp passes the raw HL7 string; the module uses `IHL7ResultParser.Parse()` to extract structured data before dispatching `ReceiveExamResultCommand`. This keeps HL7 as a swappable presentation concern.

---

## 4. Integration Events

### 4.1 Inbound — Subscriptions

**`SampleCollectedIntegrationEvent`** (emitted by SampleCollection module)

> **Cross-module dependency:** The current `SampleCollectedIntegrationEvent` carries `CollectionRequestId`, `SampleId`, `PatientId`, `SampleBarcode`, and `Exams (IReadOnlyCollection<ExamInfo>)`. The Analyzer module additionally requires `PatientName (string)`, `PatientBirthdate (DateTime)`, and `PatientGender (string)` for HL7 message construction. The SampleCollection module's `SampleCollectedIntegrationEvent` must be enriched with these three fields before this subscription can be activated.

Subscription handler: `SampleCollectedIntegrationEventNotificationHandler` (implements `INotificationHandler<SampleCollectedIntegrationEvent>`)
→ Schedules **one `CreateAnalyzerSampleBySampleCollectedCommand`** (extends `InternalCommandBase`) via `ICommandsScheduler.EnqueueAsync()`, creating a single `AnalyzerSample` aggregate that holds all exams for the sample barcode.

**`WorklistItemCreatedIntegrationEvent`** (emitted by LabAnalysis module)

Subscription handler: `WorklistItemCreatedIntegrationEventNotificationHandler` (implements `INotificationHandler<WorklistItemCreatedIntegrationEvent>`)
→ Schedules **one `AssignWorklistItemByBarcodeAndExamCodeCommand`** (extends `InternalCommandBase`) via `ICommandsScheduler.EnqueueAsync()`. The handler passes `SampleBarcode` and `ExamCode`; the command handler uses `IAnalyzerSampleByBarcodeProvider` to resolve the `AnalyzerSampleId` and dispatches the assignment.

### 4.2 Outbound — Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable, JSON-serializable.

**`ExamResultReceivedIntegrationEvent`**
- `AnalyzerSampleId` (Guid) — the Analyzer module identifier
- `WorklistItemId` (Guid) — the LabAnalysis worklist item for this exam
- `ExamMnemonic` (string) — exam code used to match the result
- `InstrumentId` (Guid) — analyzer instrument that produced the result
- `ResultValue` (string) — the measured value
- `ResultUnit` (string) — unit of measurement
- `ReferenceRange` (string) — normal range for the analyte
- `RecordedAt` (DateTime) — when the analyzer recorded the result

> Consumed by: LabAnalysis module — triggers `RecordAnalysisResultCommand` on the corresponding `WorklistItem`, advancing it from `Pending` to `ResultReceived`.

---

## 5. Read Model

### 5.1 `AnalyzerSampleDetails`

Location: `Application/AnalyzerSamples/GetAnalyzerSampleDetails/`

#### Table Schema

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `AnalyzerSampleCreatedDomainEvent` |
| `SampleId` | UUID | `AnalyzerSampleCreatedDomainEvent` |
| `PatientId` | UUID | `AnalyzerSampleCreatedDomainEvent` |
| `SampleBarcode` | VARCHAR(255) | `AnalyzerSampleCreatedDomainEvent` |
| `PatientName` | VARCHAR(255) | `AnalyzerSampleCreatedDomainEvent` |
| `PatientBirthdate` | TIMESTAMPTZ | `AnalyzerSampleCreatedDomainEvent` |
| `PatientGender` | VARCHAR(10) | `AnalyzerSampleCreatedDomainEvent` |
| `Status` | VARCHAR(50) | All state transitions |
| `DispatchedAt` | TIMESTAMPTZ NULL | `SampleInfoDispatchedDomainEvent` |
| `CreatedAt` | TIMESTAMPTZ | `AnalyzerSampleCreatedDomainEvent` |

#### Application Files

- `AnalyzerSampleDetailsDto.cs`
- `GetAnalyzerSampleDetailsQuery.cs`
- `GetAnalyzerSampleDetailsQueryHandler.cs` — Dapper SELECT
- `AnalyzerSampleDetailsProjector.cs`
  - `When(AnalyzerSampleCreatedDomainEvent)` → INSERT, Status = `"AwaitingQuery"`
  - `When(SampleInfoDispatchedDomainEvent)` → UPDATE Status = `"InfoDispatched"`, DispatchedAt
  - `When(ExamResultReceivedDomainEvent)` → UPDATE Status = `"ResultReceived"` (only when `AllResultsReceived == true`)
  - `When(IDomainEvent)` → fall-through (no-op)

### 5.2 `AnalyzerSampleExamDetails`

Location: `Application/AnalyzerSamples/GetAnalyzerSampleExamDetails/`

#### Table Schema

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `AnalyzerSampleCreatedDomainEvent` (one row per exam) |
| `AnalyzerSampleId` | UUID FK | `AnalyzerSampleCreatedDomainEvent` |
| `ExamMnemonic` | VARCHAR(255) | `AnalyzerSampleCreatedDomainEvent` |
| `WorklistItemId` | UUID NULL | `WorklistItemAssignedDomainEvent` |
| `ResultValue` | TEXT NULL | `ExamResultReceivedDomainEvent` |
| `ResultUnit` | VARCHAR(50) NULL | `ExamResultReceivedDomainEvent` |
| `ReferenceRange` | VARCHAR(255) NULL | `ExamResultReceivedDomainEvent` |
| `InstrumentId` | UUID NULL | `ExamResultReceivedDomainEvent` |
| `RecordedAt` | TIMESTAMPTZ NULL | `ExamResultReceivedDomainEvent` |

#### Application Files

- `AnalyzerSampleExamDetailsDto.cs`
- `GetAnalyzerSampleExamDetailsQuery.cs` (query by `AnalyzerSampleId`)
- `GetAnalyzerSampleExamDetailsQueryHandler.cs` — Dapper SELECT
- `AnalyzerSampleExamDetailsProjector.cs`
  - `When(AnalyzerSampleCreatedDomainEvent)` → INSERT one row per exam mnemonic
  - `When(WorklistItemAssignedDomainEvent)` → UPDATE WorklistItemId WHERE ExamMnemonic matches
  - `When(ExamResultReceivedDomainEvent)` → UPDATE ResultValue, ResultUnit, ReferenceRange, InstrumentId, RecordedAt WHERE ExamMnemonic matches
  - `When(IDomainEvent)` → fall-through (no-op)

### 5.3 `GetSampleInfoByBarcodeQuery`

Location: `Application/AnalyzerSamples/GetSampleInfoByBarcode/`

- `SampleInfoDto.cs` — includes patient demographics + list of exam DTOs
- `GetSampleInfoByBarcodeQuery.cs` — query by `SampleBarcode (string)`
- `GetSampleInfoByBarcodeQueryHandler.cs` — Dapper JOIN of `AnalyzerSampleDetails` + `AnalyzerSampleExamDetails`

> This query is used by the ConsoleApp when an analyzer scans a barcode. The result feeds into `ISampleInfoPresenter` for HL7 formatting.

---

## 6. Infrastructure Wiring

### 6.1 DomainEventTypeMappings

Register all 4 domain events in `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`:

```csharp
options.Events.AddEventType<AnalyzerSampleCreatedDomainEvent>();
options.Events.AddEventType<WorklistItemAssignedDomainEvent>();
options.Events.AddEventType<SampleInfoDispatchedDomainEvent>();
options.Events.AddEventType<ExamResultReceivedDomainEvent>();
```

### 6.2 AnalyzerStartup — OutboxModule BiMap

Register all 4 notification type mappings in `Infrastructure/Configurations/AnalyzerStartup.cs`:

```csharp
notificationsBiMap.Add("AnalyzerSampleCreatedNotification",    typeof(AnalyzerSampleCreatedNotification));
notificationsBiMap.Add("WorklistItemAssignedNotification",      typeof(WorklistItemAssignedNotification));
notificationsBiMap.Add("SampleInfoDispatchedNotification",      typeof(SampleInfoDispatchedNotification));
notificationsBiMap.Add("ExamResultReceivedNotification",        typeof(ExamResultReceivedNotification));
```

### 6.3 AnalyzerStartup — InternalCommandsModule BiMap

Register 2 internal commands in `Infrastructure/Configurations/AnalyzerStartup.cs`:

```csharp
internalCommandsMap.Add("CreateAnalyzerSampleBySampleCollectedCommand", typeof(CreateAnalyzerSampleBySampleCollectedCommand));
internalCommandsMap.Add("AssignWorklistItemByBarcodeAndExamCodeCommand", typeof(AssignWorklistItemByBarcodeAndExamCodeCommand));
```

> These commands extend `InternalCommandBase` and are scheduled by integration event handlers via `ICommandsScheduler.EnqueueAsync()`. The `ProcessInternalCommandsJob` (Quartz) picks them up and executes them asynchronously.

### 6.4 EventsBus Subscriptions

Register in `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs`:

```csharp
SubscribeToIntegrationEvent<SampleCollectedIntegrationEvent>(eventBus, logger);
SubscribeToIntegrationEvent<WorklistItemCreatedIntegrationEvent>(eventBus, logger);
```

### 6.5 Module Facade

`IAnalyzerModule` and `AnalyzerModule` already scaffolded — generic dispatcher pattern, no changes needed.

### 6.6 HL7 Infrastructure

Register in `Infrastructure/Configurations/AnalyzerStartup.cs` (or a dedicated `HL7Module` Autofac module):

```csharp
builder.RegisterType<HL7SampleInfoPresenter>().As<ISampleInfoPresenter>().SingleInstance();
builder.RegisterType<HL7ResultParser>().As<IHL7ResultParser>().SingleInstance();
```

### 6.7 Domain Provider

Register `IAnalyzerSampleByBarcodeProvider` implementation in `DataAccessModule`:

```csharp
builder.RegisterType<AnalyzerSampleByBarcodeProvider>().As<IAnalyzerSampleByBarcodeProvider>().InstancePerLifetimeScope();
```

---

## 7. Database Migrations

Location: `src/HC.LIS/HC.LIS.Database/Analyzer/`

| File | Purpose |
|---|---|
| `20260326120000_AnalyzerModule_AddSchemaAnalyzer.cs` | Already exists (from scaffold) |
| `20260326120100_AnalyzerModule_AddTableInboxMessages.cs` | Already exists (from scaffold) |
| `20260326120200_AnalyzerModule_AddTableInternalCommands.cs` | Already exists (from scaffold) |
| `20260326120300_AnalyzerModule_AddTableOutboxMessages.cs` | Already exists (from scaffold) |
| `20260409120400_AnalyzerModule_AddTableAnalyzerSampleDetails.cs` | To be created |
| `20260409120500_AnalyzerModule_AddTableAnalyzerSampleExamDetails.cs` | To be created |

---

## 8. Unit Tests

Location: `Tests/UnitTests/AnalyzerSamples/AnalyzerSampleTests.cs`
Pattern: Arrange-Act-Assert, `AssertPublishedDomainEvent<T>()` on aggregate, FluentAssertions.

| Test | Asserts |
|---|---|
| `CreateAnalyzerSampleIsSuccessful` | `AnalyzerSampleCreatedDomainEvent` raised with correct fields; exam count matches |
| `AssignWorklistItemIsSuccessful` | `WorklistItemAssignedDomainEvent` raised with correct WorklistItemId and ExamMnemonic |
| `AssignWorklistItemThrowsWhenExamDoesNotExist` | `BaseBusinessRuleException` with `ExamMustExistInSampleRule` |
| `DispatchInfoIsSuccessful` | `SampleInfoDispatchedDomainEvent` raised |
| `DispatchInfoThrowsWhenNotAwaitingQuery` | `BaseBusinessRuleException` with `CannotDispatchInfoForNonAwaitingQuerySampleRule` |
| `ReceiveExamResultIsSuccessful` | `ExamResultReceivedDomainEvent` raised with correct fields |
| `ReceiveExamResultThrowsWhenNotDispatched` | `BaseBusinessRuleException` with `CannotReceiveResultForNonDispatchedSampleRule` |
| `ReceiveExamResultThrowsWhenExamDoesNotExist` | `BaseBusinessRuleException` with `ExamMustExistInSampleRule` |
| `ReceiveLastExamResultSetsAllResultsReceivedTrue` | `ExamResultReceivedDomainEvent.AllResultsReceived == true` when final exam result recorded |

---

## 9. Integration Tests

Location: `Tests/IntegrationTests/AnalyzerSamples/`
Pattern: `TestBase.ExecuteCommandAsync(command)` → `GetEventually(probe, timeoutMs)` on projected read model.

| Test | Command Sent | Read Model Assertion |
|---|---|---|
| `CreateAnalyzerSampleIsSuccessful` | `CreateAnalyzerSampleCommand` | `AnalyzerSampleDetails.Status == "AwaitingQuery"`, all identity fields set; `AnalyzerSampleExamDetails` rows match exam count |
| `AssignWorklistItemIsSuccessful` | `AssignWorklistItemCommand` | `AnalyzerSampleExamDetails.WorklistItemId` is set for the matching exam |
| `DispatchSampleInfoIsSuccessful` | `DispatchSampleInfoCommand` | `AnalyzerSampleDetails.Status == "InfoDispatched"`, `DispatchedAt` set |
| `ReceiveExamResultIsSuccessful` | `ReceiveExamResultCommand` | `AnalyzerSampleExamDetails.ResultValue` set, `ResultUnit` set |
| `ReceiveAllExamResultsCompletesAnalyzerSample` | Multiple `ReceiveExamResultCommand` | `AnalyzerSampleDetails.Status == "ResultReceived"` after last result |

---

## 10. Open Design Decisions

| # | Decision | Options | Recommendation |
|---|---|---|---|
| 1 | `SampleCollectedIntegrationEvent` enrichment | Add `PatientName`, `PatientBirthdate`, `PatientGender` to existing event | Enrich the event in SampleCollection module — cross-module change required before Analyzer inbound subscription works |
| 2 | Urgency flag | Add `IsUrgent (bool)` to `OrderItemRequestedIntegrationEvent` and propagate through SampleCollection | Requires TestOrders domain change to support urgent orders first (PRD open question); defer to Phase 9 or a follow-up task |
| 3 | HL7 v2.x version and message types | QBP^Q11/RSP^K11 for queries, ORU^R01 for results, ACK for acknowledgment | Start with ORU^R01 (results) and a simple query/response pair; exact message types TBD with Lab Manager |
| 4 | In-process vs. separate process | In-memory event bus vs. RabbitMQ | Design for in-process first (in-memory event bus, matching all other modules); the facade pattern already supports out-of-process invocation |
| 5 | HIPAA compliance | Full audit trail vs. minimal logging | PHI stays within LIS boundary (module-to-module, LIS-to-analyzer on local network); full event sourcing provides audit trail; HIPAA applicability TBD by Lab Director |
| 6 | Analyzer brands/models | Brand-specific HL7 variations vs. generic | Module design is protocol-level (HL7 v2.x), not brand-specific; first validation target TBD by Lab Manager |
| 7 | `AnalyzerSampleExamDetails` — exam ID generation | Generated UUID per exam at creation vs. deterministic from SampleId+ExamMnemonic | Use `Guid.CreateVersion7()` per exam row during `AnalyzerSampleCreatedDomainEvent` projection |
