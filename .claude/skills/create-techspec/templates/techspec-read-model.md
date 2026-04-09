# Read model section template

## When to use

Read this template when generating **Section 5 (Read Model)** of the tech spec.

---

## Section format

```markdown
## 5. Read Model: `{ReadModelName}`

Location: `Application/{Aggregates}/Get{Aggregate}Details/`

### 5.1 Table Schema

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `{CreatedEvent}` |
| `{Column}` | {SQL_TYPE} | `{Event}` |

### 5.2 Application Files

- `{ReadModelName}Dto.cs`
- `Get{ReadModelName}Query.cs`
- `Get{ReadModelName}QueryHandler.cs` — Dapper SELECT
- `{ReadModelName}Projector.cs`
  - `When({Event})` → {SQL operation}, {field assignments}
```

---

## Conventions

### Table schema

- Table name: `{AggregateName}Details` (e.g., `WorklistItemDetails`, `OrderDetails`)
- Schema: `{module_schema}` (snake_case of ModuleName)
- Column types mapping:

| C# Type | SQL Type | Notes |
|---|---|---|
| `Guid` | `UUID` | PK for aggregate ID |
| `string` (short) | `VARCHAR(N)` | 50 for status, 255 for names/codes, 500 for paths |
| `string` (long) | `TEXT` | For unbounded text like result values |
| `DateTime` | `TIMESTAMPTZ` | Always with timezone |
| `bool` | `BOOLEAN` | |
| `decimal` | `NUMERIC(p,s)` | Specify precision |

- Nullable columns: append `NULL` for fields not set at creation (e.g., `ResultValue TEXT NULL`)
- Every column documents which domain event populates it in the "Populated by" column
- The `Status` column is always `VARCHAR(50)` and is updated by every state-changing event

### DTO

- Name: `{AggregateName}DetailsDto`
- Immutable record with nullable properties for optional fields
- Properties match the table columns 1:1

### Query

- Name: `Get{AggregateName}DetailsQuery(Guid {aggregateCamelCase}Id)`
- Extends `IQuery<{AggregateName}DetailsDto?>`
- Returns nullable DTO (query may find no results)

### Query handler

- Name: `Get{AggregateName}DetailsQueryHandler`
- Injects `ISqlConnectionFactory`
- Uses Dapper: `QueryFirstOrDefaultAsync<{Dto}>` with parameterized SQL
- SQL selects from `"{module_schema}"."{TableName}"`
- `.ConfigureAwait(false)` on the Dapper call

### Projector

- Name: `{AggregateName}DetailsProjector`
- Extends `ProjectorBase`
- One `When({EventType})` method per domain event:
  - Creation event → `INSERT INTO`
  - Mutation event → `UPDATE ... SET ... WHERE "Id" = @Id`
  - Fall-through: `When(IDomainEvent)` → no-op
- SQL uses Dapper `ExecuteAsync` with parameterized queries
- Column names in SQL are double-quoted (PostgreSQL)

---

## Example (from LabAnalysis-TechSpec.md)

```markdown
### 5.1 Table Schema

| Column | Type | Populated by |
|---|---|---|
| `Id` | UUID PK | `WorklistItemCreatedDomainEvent` |
| `SampleId` | UUID | `WorklistItemCreatedDomainEvent` |
| `SampleBarcode` | VARCHAR(255) | `WorklistItemCreatedDomainEvent` |
| `ExamCode` | VARCHAR(255) | `WorklistItemCreatedDomainEvent` |
| `PatientId` | UUID | `WorklistItemCreatedDomainEvent` |
| `Status` | VARCHAR(50) | All state transitions |
| `ResultValue` | TEXT NULL | `AnalysisResultRecordedDomainEvent` |
| `ReportPath` | VARCHAR(500) NULL | `ReportGeneratedDomainEvent` |
| `CompletionType` | VARCHAR(50) NULL | `WorklistItemCompletedDomainEvent` |
| `CreatedAt` | TIMESTAMPTZ | `WorklistItemCreatedDomainEvent` |
| `CompletedAt` | TIMESTAMPTZ NULL | `WorklistItemCompletedDomainEvent` |

### 5.2 Application Files

- `WorklistItemDetailsDto.cs`
- `GetWorklistItemDetailsQuery.cs`
- `GetWorklistItemDetailsQueryHandler.cs` — Dapper SELECT
- `WorklistItemDetailsProjector.cs`
  - `When(WorklistItemCreatedDomainEvent)` → INSERT, Status = `"Pending"`
  - `When(AnalysisResultRecordedDomainEvent)` → UPDATE ResultValue, Status = `"ResultReceived"`
  - `When(ReportGeneratedDomainEvent)` → UPDATE ReportPath, Status = `"ReportGenerated"`
  - `When(WorklistItemCompletedDomainEvent)` → UPDATE Status = `"Completed"`, CompletionType, CompletedAt
  - `When(IDomainEvent)` → fall-through (no-op)
```

---

## Checklist before moving to Section 6

- [ ] Every domain event from Section 2.5 appears as a "Populated by" source in at least one column
- [ ] The creation event populates all non-nullable columns
- [ ] Nullable columns are only populated by subsequent events
- [ ] Status column is updated by every state-changing event
- [ ] Projector has one `When()` per domain event plus a fall-through `When(IDomainEvent)`
- [ ] DTO properties match table columns 1:1
