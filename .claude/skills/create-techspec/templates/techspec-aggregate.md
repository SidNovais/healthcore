# Aggregate section template

## When to use

Read this template when generating **Section 2 (Aggregate)** of the tech spec.

---

## Section format

```markdown
## 2. Aggregate: `{AggregateName}`

### 2.1 Identity

`{AggregateName}Id` — typed ID wrapping `Guid`, string-keyed in Marten (`StreamIdentity.AsString`).

### 2.2 State Machine

{ASCII diagram — use arrow notation}

| Status | Meaning |
|---|---|
| `{Status}` | {One-sentence meaning} |

### 2.3 Domain Methods & Events

| Method | Business Rule(s) | Domain Event Emitted |
|---|---|---|
| `{Method}({params})` | `{RuleName}` | `{EventName}` |

### 2.4 Business Rules

| Class | Invariant |
|---|---|
| `{RuleName}` | {Plain-English invariant} |

### 2.5 Domain Events (fields)

**`{EventName}`**
- `{PropertyName}` ({PrimitiveType}), ...
```

---

## Conventions

### Identity
- Always a typed ID class: `{AggregateName}Id` wrapping `Guid`
- String-keyed in Marten: `StreamIdentity.AsString`
- Example: `WorklistItemId`, `OrderId`

### State machine
- ASCII diagram uses `→` for transitions: `Pending → ResultReceived → ReportGenerated → Completed`
- For branching paths, use vertical layout:
  ```
  Pending → Active → Completed
                   ↘ Cancelled
  ```
- Every status is a `ValueObject` — never a plain string
- Create `{AggregateName}Status` class (see `/domain` templates for pattern)
- Each status in the diagram must appear in the status table

### Domain methods
- Creation method: always a static `Create(...)` factory — never a public constructor
- Mutation methods: instance methods that call `Apply()` + `AddDomainEvent()`
- Method signature includes all parameters needed for the domain event
- Method params: use primitives and typed IDs, not value objects (the aggregate unwraps internally)
- Every method emits exactly one domain event
- Every state transition has at least one business rule guarding it (except `Create`)

### Business rules
- Name pattern: `Cannot{Action}{Condition}Rule` (e.g., `CannotRecordResultForNonPendingWorklistItemRule`)
- Each rule checks one invariant
- Exception + rule live in the same file
- Rule accepts current state as constructor param; `IsBroken()` checks against it

### Domain events (fields)
- **Primitives only:** `Guid`, `string`, `DateTime`, `bool`, `int`, `decimal`
- **Never** include ValueObjects, entities, or collections in event fields
- Unwrap value objects: `_status.Value` → `string Status` in the event
- Include the aggregate ID as the first field
- Include a timestamp field for the action (e.g., `RecordedAt`, `CompletedAt`)

---

## Example (from LabAnalysis-TechSpec.md)

```markdown
### 2.1 Identity

`WorklistItemId` — typed ID wrapping `Guid`, string-keyed in Marten (`StreamIdentity.AsString`).

### 2.2 State Machine

Pending → ResultReceived → ReportGenerated → Completed

| Status | Meaning |
|---|---|
| `Pending` | Worklist item created; awaiting result from clinical analyzer |
| `ResultReceived` | Result recorded; report not yet generated |
| `ReportGenerated` | PDF report stored; item not yet formally completed |
| `Completed` | Notification sent to TestOrders; no further state changes allowed |

### 2.3 Domain Methods & Events

| Method | Business Rule(s) | Domain Event Emitted |
|---|---|---|
| `Create(Guid worklistItemId, Guid sampleId, string sampleBarcode, string examCode, Guid patientId, DateTime createdAt)` | — | `WorklistItemCreatedDomainEvent` |
| `RecordResult(string resultValue, Guid analystId, DateTime recordedAt)` | `CannotRecordResultForNonPendingWorklistItemRule` | `AnalysisResultRecordedDomainEvent` |
```

---

## Checklist before moving to Section 3

- [ ] Every status in the state machine has a row in the status table
- [ ] Every method emits exactly one domain event
- [ ] Every state transition (except Create) has at least one business rule
- [ ] Every domain event lists only primitive types — no ValueObjects or entity references
- [ ] Aggregate ID is the first field in every domain event
- [ ] A timestamp field is present in every domain event
- [ ] The state machine is a DAG — no cycles (unless explicitly justified)
