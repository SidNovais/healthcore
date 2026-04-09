# Integration events section template

## When to use

Read this template when generating **Section 4 (Integration Events)** of the tech spec.

---

## Section format

```markdown
## 4. Integration Events

### 4.1 Inbound — Subscriptions

**`{SourceEventName}`** (emitted by {SourceModule} module)

{Cross-module dependency callout if needed — see below}

Subscription handler: `{SourceEventName}IntegrationEventHandler` (implements `IIntegrationEventHandler<{SourceEventName}>`)
→ Dispatches `{CommandName}` with fields from integration event payload.

### 4.2 Outbound — Integration Events

Location: `IntegrationEvents/`
Pattern: inherit `IntegrationEvent(Guid id, DateTime occurredAt)`, immutable, JSON-serializable.

**`{OutboundEventName}`**
- `{PropertyName}` ({PrimitiveType}) — {purpose}

> Consumed by: {ConsumerModule} — {trigger description}.
```

---

## Conventions

### Inbound subscriptions

- For each inbound integration event, specify:
  1. **Source module** — which module emits the event
  2. **Event class name** — the full `{Event}IntegrationEvent` name
  3. **Handler class name** — `{Event}IntegrationEventNotificationHandler` (CA1711)
  4. **Handler file location** — co-located with the command it dispatches
  5. **Command(s) dispatched** — what the handler creates and sends

### Cross-module enrichment callout

When an inbound integration event does NOT carry all the data the new module needs, insert a callout block:

```markdown
> **Cross-module dependency:** The current `{EventName}` only carries {existing fields}. {ModuleName} requires {missing fields}. The {SourceModule} module's `{EventName}` must be enriched with {list of new fields} before this subscription can be activated.
```

This is critical — it surfaces integration blockers before implementation begins.

### Fan-out pattern

When one inbound event should create multiple aggregates (e.g., one collected sample with N exam codes):

```markdown
Subscription handler: `{EventName}IntegrationEventHandler`
→ Iterates `integrationEvent.{CollectionField}` and dispatches **one `{CommandName}` per {item}**, each with a freshly generated `{AggregateId}`.
```

### Outbound integration events

- Inherit `IntegrationEvent(Guid id, DateTime occurredAt)`
- Constructor parameters match all properties (JSON serialization)
- Fields are primitives only (Guid, string, DateTime, etc.)
- Published via `IEventsBus.Publish(event)` from the publish notification handler
- For each event, document:
  1. **Class name** — `{Aggregate}{Action}IntegrationEvent`
  2. **Fields** — all properties with types
  3. **Consumer** — which module or system consumes it and why

---

## Example (from LabAnalysis-TechSpec.md)

```markdown
### 4.1 Inbound — Subscriptions

**`SampleCollectedIntegrationEvent`** (emitted by SampleCollection module)

> **Cross-module dependency:** The current `SampleCollectedIntegrationEvent` only carries `CollectionRequestId` and `SampleId`. LabAnalysis requires `SampleBarcode`, `PatientId`, and — critically — **a collection of exam codes**, because a single collected sample may have multiple exams ordered on it, each of which produces a distinct analyte and must become its own `WorklistItem`. The SampleCollection module's `SampleCollectedIntegrationEvent` must be enriched with `SampleBarcode (string)`, `PatientId (Guid)`, and `ExamCodes (IReadOnlyCollection<string>)` before this subscription can be activated.

Subscription handler: `SampleCollectedIntegrationEventHandler` (implements `IIntegrationEventHandler<SampleCollectedIntegrationEvent>`)
→ Iterates `integrationEvent.ExamCodes` and dispatches **one `CreateWorklistItemCommand` per exam code**, each with a freshly generated `WorklistItemId`.

### 4.2 Outbound — Integration Events

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
```

---

## Checklist before moving to Section 5

- [ ] Every inbound integration event has a handler class and a command it dispatches
- [ ] Cross-module enrichment dependencies are explicitly called out with exact fields needed
- [ ] Fan-out patterns are documented when one event creates multiple aggregates
- [ ] Every outbound event lists its consumer(s) and trigger description
- [ ] Outbound event fields are primitives only
- [ ] Handler class names use `NotificationHandler` suffix (CA1711), not `EventHandler`
