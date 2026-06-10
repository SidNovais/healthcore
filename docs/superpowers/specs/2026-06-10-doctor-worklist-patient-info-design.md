# Design: Doctor Worklist — Patient Name, DOB & Gender

## Context

The `LabAnalysis` module's worklist currently exposes only `patient_id` (a UUID) in both list and detail views. Physicians using the worklist must identify patients by raw GUID, which is unusable in practice. The fix is to propagate patient demographics — full name, date of birth, and gender — into the LabAnalysis read model by following the established pattern already used by TestOrders.

`PatientManagement` publishes three integration events (`PatientRegistered`, `PatientUpdated`, `PatientAnonymized`). TestOrders already subscribes to these and maintains a `test_orders."PatientSnapshotDetails"` table. LabAnalysis will do the same, then JOIN that local snapshot in both worklist queries.

---

## Architecture

### Data Flow

```
PatientManagement (integration events)
  → LabAnalysis integration event handlers
    → ICommandsScheduler.EnqueueAsync(internal command)
      → ProcessInternalCommandsJob (Quartz)
        → Command handler → IPatientSnapshotRepository
          → lab_analysis."PatientSnapshotDetails" table
```

At read time, `GetWorklistItemListQueryHandler` and `GetWorklistItemDetailsQueryHandler` LEFT JOIN `worklist_item_details` with `PatientSnapshotDetails` on `patient_id`. The projected `worklist_item_details` table itself is unchanged.

### Module Boundary

LabAnalysis owns its own copy of patient data. No cross-schema query to `patient_management.*` at any point.

---

## Components

### 1. DB Migration

**New file:** `src/HC.LIS/HC.LIS.Database/Migrations/{timestamp}_LabAnalysisModule_AddTablePatientSnapshotDetails.cs`

Creates `lab_analysis."PatientSnapshotDetails"` with the same schema as `test_orders."PatientSnapshotDetails"`:

| Column | Type |
|---|---|
| Id | UUID (PK) |
| FullName | VARCHAR NOT NULL |
| DateOfBirth | TIMESTAMPTZ NOT NULL |
| Gender | VARCHAR NULL |
| MothersFullName | VARCHAR NULL |
| DocumentId | VARCHAR NULL |
| Phone | VARCHAR NULL |
| Email | VARCHAR NULL |
| Status | VARCHAR NOT NULL |
| RegisteredAt | TIMESTAMPTZ NOT NULL |
| AnonymizedAt | TIMESTAMPTZ NULL |

---

### 2. Application — Internal Commands (3 new commands)

Mirror the TestOrders pattern exactly. Each command extends `InternalCommandBase` with `[method: JsonConstructor]`.

**Store:**
`Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommand.cs`
Props: PatientId, FullName, DateOfBirth, Gender, MothersFullName, DocumentId, Phone, Email, RegisteredAt

`Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommandHandler.cs`
Calls `IPatientSnapshotRepository.StoreAsync()`

**Update:**
`Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommand.cs`
Props: PatientId, FullName, DateOfBirth, Gender, MothersFullName, DocumentId, Phone, Email, UpdatedAt

`Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommandHandler.cs`
Calls `IPatientSnapshotRepository.UpdateAsync()`

**Anonymize:**
`Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommand.cs`
Props: PatientId, AnonymizedAt

`Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommandHandler.cs`
Calls `IPatientSnapshotRepository.AnonymizeAsync()`

---

### 3. Application — Integration Event Handlers (3 new handlers)

Subscribe to PatientManagement integration events. Each injects `ICommandsScheduler` and enqueues the matching internal command.

`Application/Patients/StorePatientSnapshot/PatientRegisteredIntegrationEventNotificationHandler.cs`
`Application/Patients/UpdatePatientSnapshot/PatientUpdatedIntegrationEventNotificationHandler.cs`
`Application/Patients/AnonymizePatientSnapshot/PatientAnonymizedIntegrationEventNotificationHandler.cs`

**Reference:** `TestOrders/Application/Patients/StorePatientSnapshot/PatientRegisteredIntegrationEventNotificationHandler.cs`

---

### 4. Application — Repository Interface

`Application/Patients/IPatientSnapshotRepository.cs`

Mirror exact method signatures from `TestOrders/Application/Patients/IPatientSnapshotRepository.cs`.

---

### 5. Infrastructure — Repository Implementation

`Infrastructure/Patients/PatientSnapshotRepository.cs`

Dapper implementation writing to `lab_analysis."PatientSnapshotDetails"`. Mirror `TestOrders/Infrastructure/Patients/PatientSnapshotRepository.cs` exactly, changing only the schema prefix.

---

### 6. Infrastructure — Register Internal Commands

In `LabAnalysisStartup.cs`, add the 3 new commands to the internal commands BiMap alongside existing ones:

```csharp
.Add(nameof(StorePatientSnapshotByPatientIdCommand), typeof(StorePatientSnapshotByPatientIdCommand))
.Add(nameof(UpdatePatientSnapshotByPatientIdCommand), typeof(UpdatePatientSnapshotByPatientIdCommand))
.Add(nameof(AnonymizePatientSnapshotByPatientIdCommand), typeof(AnonymizePatientSnapshotByPatientIdCommand))
```

---

### 7. Query Updates — List View

**File:** `Application/WorklistItems/GetWorklistItemList/GetWorklistItemListQueryHandler.cs`

Update SQL to LEFT JOIN patient snapshot (column names follow the TestOrders `PatientSnapshotDetails` convention — quoted PascalCase in PostgreSQL, mapped by Dapper):

```sql
SELECT
    wid.id             AS "Id",
    wid.sample_barcode AS "SampleBarcode",
    wid.exam_code      AS "ExamCode",
    wid.patient_id     AS "PatientId",
    psd."FullName"     AS "PatientName",
    psd."DateOfBirth"  AS "PatientDateOfBirth",
    psd."Gender"       AS "PatientGender",
    wid.status         AS "Status",
    wid.created_at     AS "CreatedAt"
FROM lab_analysis.worklist_item_details AS wid
LEFT JOIN lab_analysis."PatientSnapshotDetails" AS psd ON psd."Id" = wid.patient_id
WHERE (@Status IS NULL OR wid.status = @Status)
ORDER BY wid.created_at
```

**File:** `Application/WorklistItems/GetWorklistItemList/WorklistItemSummaryDto.cs`

Add 3 new nullable properties: `PatientName`, `PatientDateOfBirth`, `PatientGender` (nullable to handle the eventual consistency window between worklist item creation and snapshot arrival).

---

### 8. Query Updates — Detail View

**File:** `Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQueryHandler.cs`

Add the same LEFT JOIN to the existing QueryMultiple call. Add PatientName, PatientDateOfBirth, PatientGender columns to the first result set.

**File:** `Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsDto.cs`

Add the same 3 nullable properties.

---

### 9. Frontend

**`WorklistItemSummary` interface** (`src/app/core/domain/worklist-item-summary.ts`):
Add `patientName: string | null`, `patientDateOfBirth: string | null`, `patientGender: string | null`

**`WorklistItemDetails` interface** (`src/app/core/domain/worklist-item-details.ts`):
Add the same 3 fields.

**`WorklistComponent` template** (`src/app/features/worklist/worklist.component.ts`):
Replace the `Patient ID` column header and `{{ item.patientId }}` cell with `Patient` / `{{ item.patientName ?? item.patientId }}` (GUID fallback handles the eventual consistency window gracefully).

**`WorklistItemDetailComponent` template**:
Replace `<p>Patient ID: {{ item.patientId }}</p>` with:
```html
<p>Patient: {{ item.patientName }}</p>
<p>Date of birth: {{ item.patientDateOfBirth | date:'shortDate' }}</p>
<p>Gender: {{ item.patientGender }}</p>
```

---

## Verification

1. Run `dotnet test` on LabAnalysis UnitTests and IntegrationTests
2. Start DB: `docker-compose -f development-compose.yaml up -d` and run migrations: `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj`
3. Register a patient via the receptionist flow
4. Create a test order to generate a worklist item
5. Confirm `lab_analysis."PatientSnapshotDetails"` is populated
6. Open the Angular SPA at `http://localhost:4200`, navigate to the physician worklist — confirm patient name appears
7. Open a worklist item detail — confirm name, DOB, and gender are displayed
8. Run E2E tests: `yarn e2e` in `src/HC.LIS.Frontend/packages/hc-lis-spa`
