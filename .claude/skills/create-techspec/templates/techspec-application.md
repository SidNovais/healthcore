# Application layer section template

## When to use

Read this template when generating **Section 3 (Application Layer)** of the tech spec.

---

## Section format

```markdown
## 3. Application Layer

### 3.1 Commands

Location: `Application/{Aggregates}/{CommandName}/`

| Command | Properties | Aggregate Method Called |
|---|---|---|
| `{CommandName}` | `{Prop} ({Type})` — extends `CommandBase<Guid>` | `{Aggregate}.{Method}(...)` via `IAggregateStore.Start()` |

### 3.2 Notifications

| Notification | Co-located With | Integration Event? |
|---|---|---|
| `{NotificationName}` | `{CommandFolder}/` | **Yes** — emits `{IntegrationEvent}` / No |

### 3.3 Notification Projection Handlers

| Projection Class | Co-located With | Read Model Updated |
|---|---|---|
| `{ProjectionName}` | `{CommandFolder}/` | `{ReadModel}` — {SQL operation} |
```

---

## Conventions

### Commands

| Scenario | Base class | Return type | Store method |
|---|---|---|---|
| Create aggregate | `CommandBase<Guid>` | Aggregate ID | `IAggregateStore.Start(aggregate)` |
| Mutate aggregate | `CommandBase` | void | `IAggregateStore.AppendChanges(aggregate)` |

- Command name: `{Action}{Aggregate}Command` (e.g., `CreateWorklistItemCommand`, `RecordAnalysisResultCommand`)
- Command handler name: `{Action}{Aggregate}CommandHandler`
- One command per folder under `Application/{Aggregates}/{ActionAggregate}/`
- Command properties match the aggregate method params (primitives + typed IDs)
- Use primary constructor syntax

### Notifications

- One notification per domain event
- Name: `{Aggregate}{Action}Notification` (e.g., `WorklistItemCreatedNotification`)
- Co-located in the same folder as the command that triggers it
- Each notification wraps a `DomainEventNotificationBase<{DomainEvent}>`

### Notification projection handlers

- Name: `{Aggregate}{Action}NotificationProjection` (e.g., `WorklistItemCreatedNotificationProjection`)
- Co-located with the command folder — NOT in the read model folder
- Each projection handler updates the read model via the projector
- SQL operations: INSERT for creation events, UPDATE for mutation events

### Publish event notification handlers

- Only for notifications that emit integration events
- Name: `{Aggregate}{Action}PublishEventNotificationHandler` (e.g., `WorklistItemCreatedPublishEventNotificationHandler`)
- Co-located with the command folder
- Uses `notification.DomainEvent.{Prop}` inline — no intermediate variable

### Integration event handlers (inbound)

- Live in the command folder of the command they dispatch
- File name: `{SourceEvent}IntegrationEventHandler.cs`
- Class name: `{SourceEvent}IntegrationEventNotificationHandler` (CA1711 forbids `EventHandler` suffix on type names)
- Example: `Application/WorklistItems/CreateWorklistItem/SampleCollectedIntegrationEventHandler.cs` containing class `SampleCollectedIntegrationEventNotificationHandler`

---

## Example (from LabAnalysis-TechSpec.md)

```markdown
### 3.1 Commands

| Command | Properties | Aggregate Method Called |
|---|---|---|
| `CreateWorklistItemCommand` | `WorklistItemId (Guid)`, `SampleId (Guid)`, `SampleBarcode (string)`, `ExamCode (string)`, `PatientId (Guid)`, `CreatedAt (DateTime)` — extends `CommandBase<Guid>` | `WorklistItem.Create(...)` via `IAggregateStore.Start()` |
| `RecordAnalysisResultCommand` | `WorklistItemId (Guid)`, `ResultValue (string)`, `AnalystId (Guid)`, `RecordedAt (DateTime)` — extends `CommandBase` | `WorklistItem.RecordResult(...)` via `AppendChanges` |

### 3.2 Notifications

| Notification | Co-located With | Integration Event? |
|---|---|---|
| `WorklistItemCreatedNotification` | `CreateWorklistItem/` | **Yes** — emits `WorklistItemCreatedIntegrationEvent` to clinical analyzer |
| `AnalysisResultRecordedNotification` | `RecordAnalysisResult/` | No |
```

---

## Checklist before moving to Section 4

- [ ] One command per domain method (1:1 mapping from Section 2.3)
- [ ] One notification per domain event
- [ ] One projection handler per notification
- [ ] Publish handlers only for notifications that emit integration events
- [ ] Integration event handlers placed in the correct command folder
- [ ] All class names follow CA1711 (no `EventHandler` suffix on types)
- [ ] Command base class is correct: `CommandBase<Guid>` for Create, `CommandBase` for mutations
