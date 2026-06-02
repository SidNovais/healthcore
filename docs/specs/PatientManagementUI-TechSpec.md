# Technical Spec: PatientManagement UI

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-06-02
**Backend Spec Reference:** [docs/specs/PatientManagement-TechSpec.md](./PatientManagement-TechSpec.md)
**PRD Reference:** [docs/prd/PatientManagement.md](../prd/PatientManagement.md)

---

## 1. Overview

The PatientManagement UI surfaces the patient registry to Receptionists and IT Administrators through three Angular screens: search, registration, and detail (with edit and anonymize). The PatientManagement backend module is fully implemented but not yet wired into the API — this spec covers both the API endpoint layer and the Angular frontend.

**Key workflows:**
- **Receptionist** — searches for an existing patient before placing an order; registers a new patient if none is found; edits patient demographics.
- **ITAdmin** — does all of the above, plus can irreversibly anonymize a patient to satisfy right-to-be-forgotten requests.

**Technology:** Same stack as HealthcoreSPA — Angular 21 (standalone), TypeScript, Signals, `@hey-api/openapi-ts`-generated SDK, Playwright E2E.

---

## 2. Backend API Prerequisites

### 2.1 New Authorization Policy

Add to `Program.cs` `AddAuthorization()`:

```csharp
options.AddPolicy("PatientManagement", policy =>
    policy.RequireRole("Receptionist", "ITAdmin"));
```

### 2.2 Module Wiring

Add to `Program.cs` Autofac registration block:
```csharp
containerBuilder.RegisterModule(new PatientManagementAutofacModule());
```

Add to `Program.cs` module initialization block:
```csharp
PatientManagementStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);
```

Add to `Program.cs` versioned endpoint groups:
```csharp
v1.MapGroup("patients").MapPatientsEndpoints();
```

### 2.3 Endpoints

All five endpoints live under `src/HC.LIS/HC.LIS.API/Modules/PatientManagement/Patients/`.

#### `POST /api/v1/patients` — Register Patient

- **Auth:** `.RequireAuthorization("PatientManagement")`
- **Request body:** `RegisterPatientRequest` (`FullName`, `DateOfBirth`, `Gender?`, `MothersFullName?`, `DocumentId?`, `Phone?`, `Email?`)
- **Handler:** creates `PatientId = Guid.CreateVersion7()`, `RegisteredAt = SystemClock.Now`; calls `module.ExecuteCommandAsync(new RegisterPatientCommand(...))`.
- **Response:** `201 Created` with `CreatedIdResponse(patientId)` and `Location: /api/v1/patients/{id}`; `400` on validation error.

#### `GET /api/v1/patients?search={term}` — Search Patients

- **Auth:** `.RequireAuthorization("PatientManagement")`
- **Query param:** `string search` (required, injected from query string)
- **Handler:** calls `module.ExecuteQueryAsync(new SearchPatientsQuery($"%{search}%"))` (the `%` wildcards are added server-side).
- **Response:** `200 OK` with `IReadOnlyCollection<PatientSearchResultDto>`

#### `GET /api/v1/patients/{id:guid}` — Get Patient Details

- **Auth:** `.RequireAuthorization("PatientManagement")`
- **Handler:** calls `module.ExecuteQueryAsync(new GetPatientDetailsQuery(id))`.
- **Response:** `200 OK` with `PatientDetailsDto`; `404` if not found.

#### `PUT /api/v1/patients/{id:guid}` — Update Patient

- **Auth:** `.RequireAuthorization("PatientManagement")`
- **Request body:** `UpdatePatientRequest` (`FullName`, `DateOfBirth`, `Gender?`, `MothersFullName?`, `DocumentId?`, `Phone?`, `Email?`)
- **Handler:** `UpdatedAt = SystemClock.Now`; calls `module.ExecuteCommandAsync(new UpdatePatientCommand(...))`.
- **Response:** `204 No Content`; `400` on business rule violation (patient is Anonymized); `409` on conflict.

#### `POST /api/v1/patients/{id:guid}/anonymize` — Anonymize Patient

- **Auth:** `.RequireAuthorization("ITAdmin")` — ITAdmin only
- **Handler:** `AnonymizedAt = SystemClock.Now`; calls `module.ExecuteCommandAsync(new AnonymizePatientCommand(id, anonymizedAt))`.
- **Response:** `204 No Content`; `400` on business rule violation (already Anonymized); `409` on conflict.

### 2.4 Endpoint Registration (`PatientsEndpoints.cs`)

```csharp
internal static class PatientsEndpoints
{
    internal static RouteGroupBuilder MapPatientsEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("Patients");

        group.MapPost("", RegisterPatientEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("RegisterPatient")
            .WithSummary("Register a new patient.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("", SearchPatientsEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("SearchPatients")
            .WithSummary("Search patients by name or document ID.")
            .Produces<IReadOnlyCollection<PatientSearchResultDto>>();

        group.MapGet("{id:guid}", GetPatientDetailsEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("GetPatientDetails")
            .WithSummary("Get patient details by ID.")
            .Produces<PatientDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}", UpdatePatientEndpoint.Handle)
            .RequireAuthorization("PatientManagement")
            .WithName("UpdatePatient")
            .WithSummary("Update patient demographics.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/anonymize", AnonymizePatientEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("AnonymizePatient")
            .WithSummary("Irreversibly anonymize a patient record.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
```

---

## 3. Angular Clean Architecture

Follows the 4-layer pattern from HealthcoreSPA-TechSpec §2.4. Patients layer mirrors the existing `orders/` and `users/` layers exactly.

### 3.1 Domain (`core/domain/`)

**`patient-details.ts`**
```typescript
export interface PatientDetails {
  id: string;
  fullName: string;
  dateOfBirth: string;       // ISO 8601 date
  gender: string | null;
  mothersFullName: string | null;
  documentId: string | null;
  phone: string | null;
  email: string | null;
  status: 'Active' | 'Anonymized';
  registeredAt: string;
  anonymizedAt: string | null;
}
```

**`patient-search-result.ts`**
```typescript
export interface PatientSearchResult {
  id: string;
  fullName: string;
  dateOfBirth: string;
  documentId: string | null;
  status: 'Active' | 'Anonymized';
}
```

**`register-patient-params.ts`** and **`update-patient-params.ts`** (param types for the port interface):
```typescript
export interface RegisterPatientParams {
  fullName: string;
  dateOfBirth: string;
  gender?: string;
  mothersFullName?: string;
  documentId?: string;
  phone?: string;
  email?: string;
}
// UpdatePatientParams is identical in shape to RegisterPatientParams
export type UpdatePatientParams = RegisterPatientParams;
```

### 3.2 Application (`core/application/`)

**`i-patients-port.ts`**
```typescript
export interface IPatientsPort {
  search(term: string): Promise<PatientSearchResult[]>;
  getDetails(id: string): Promise<PatientDetails>;
  register(data: RegisterPatientParams): Promise<string>;
  update(id: string, data: UpdatePatientParams): Promise<void>;
  anonymize(id: string): Promise<void>;
}
```

**`patients.service.ts`** — `@Injectable({ providedIn: 'root' })`

| Signal | Type | Description |
|---|---|---|
| `searchResults` | `Signal<PatientSearchResult[]>` | Current search result list |
| `patient` | `Signal<PatientDetails \| null>` | Currently loaded patient detail |
| `error` | `Signal<string \| null>` | Last error message |

Methods: `search(term)`, `loadDetails(id)`, `register(data)` → returns `Promise<string>` (new id), `update(id, data)`, `anonymize(id)`.

### 3.3 Infrastructure (`core/infrastructure/patients/`)

**`sdk-patients-adapter.ts`** — implements `IPatientsPort`; the only layer that imports `@hc-lis/api-client` generated SDK functions for patients. Pattern mirrors `sdk-orders-adapter.ts`.

Provide via DI token in `app.config.ts`:
```typescript
{ provide: PATIENTS_PORT, useClass: SdkPatientsAdapter }
```

---

## 4. Screens & Features

### 4.1 Patient Search — `PatientSearchComponent`

**Route:** `/patients`

**Behavior:**
- Text input debounced 300 ms triggers `PatientsService.search(term)`.
- Results rendered in a table: Full Name, Date of Birth, Document ID, Status badge.
- Empty state displayed when `searchResults()` is empty and a search has been submitted.
- "Register new patient" button navigates to `/patients/new`.
- Clicking a result row navigates to `/patients/:id`.

**`data-testid` attributes:**

| Attribute | Element |
|---|---|
| `patient-search-input` | Text input |
| `patient-search-results` | Results `<table>` or list container |
| `patient-row` | Each result row `<tr>` or item |
| `register-patient-btn` | Register button |
| `patient-search-empty-state` | Empty-state element |

### 4.2 Register Patient — `RegisterPatientComponent`

**Route:** `/patients/new`

**Behavior:**
- Renders the shared `PatientFormComponent` in "register" mode.
- On valid submit → `PatientsService.register(data)` → navigate to `/patients/:newId`.
- Shows inline error if registration fails.

**`data-testid` attributes:**

| Attribute | Element |
|---|---|
| `register-patient-heading` | Page title |
| `register-patient-error` | Error message container |

### 4.3 Patient Detail — `PatientDetailComponent`

**Route:** `/patients/:id`

**Behavior:**
- On init, calls `PatientsService.loadDetails(id)`.
- Displays all `PatientDetails` fields in a read-only view.
- **Status badge:** "Active" (shown in green) / "Anonymized" (shown in gray).
- **Edit:** "Edit" button toggles inline `PatientFormComponent`; pre-populated with current values. Button hidden when status is `Anonymized`.
- **Anonymize:** "Anonymize" button visible only to ITAdmin (`currentUser().role === 'ITAdmin'`). Clicking shows an inline confirmation step (a second "Confirm anonymize" button). Disabled and hidden once already `Anonymized`. On confirm → `PatientsService.anonymize(id)` → reload details.

**`data-testid` attributes:**

| Attribute | Element |
|---|---|
| `patient-status-badge` | Status badge span |
| `patient-edit-btn` | "Edit" toggle button |
| `patient-anonymize-btn` | "Anonymize" button (ITAdmin only) |
| `anonymize-confirm-btn` | Confirmation button (shown after clicking anonymize) |
| `patient-detail-error` | Error message container |

### 4.4 Shared Patient Form — `PatientFormComponent`

Reused as a child component in both `RegisterPatientComponent` and `PatientDetailComponent` (edit mode). Accepts `@Input() initialValues` for pre-population and emits `@Output() formSubmit: EventEmitter<RegisterPatientParams | UpdatePatientParams>`.

**Fields:**

| Field | Input Type | Required |
|---|---|---|
| Full Name | `text` | Yes |
| Date of Birth | `date` | Yes |
| Gender | `select` (Male, Female, Other) | No |
| Mother's Full Name | `text` | No |
| Document ID | `text` | No |
| Phone | `tel` | No |
| Email | `email` | No |

**`data-testid` attributes:**

| Attribute | Element |
|---|---|
| `patient-full-name-input` | Full Name `<input>` |
| `patient-dob-input` | Date of Birth `<input>` |
| `patient-gender-select` | Gender `<select>` |
| `patient-mothers-name-input` | Mother's Full Name `<input>` |
| `patient-document-id-input` | Document ID `<input>` |
| `patient-phone-input` | Phone `<input>` |
| `patient-email-input` | Email `<input>` |
| `patient-form-submit-btn` | Submit `<button>` |

---

## 5. `roleGuard` Extension

The existing `roleGuard` accepts a single `UserRole`. Extend to accept rest parameters so the three patient routes can permit both Receptionist and ITAdmin without a new guard:

```typescript
// Before
export function roleGuard(role: UserRole): CanActivateFn

// After
export function roleGuard(...roles: UserRole[]): CanActivateFn
```

Existing single-role call sites (`roleGuard('Receptionist')`) remain valid — rest params are backward compatible.

**File:** `src/app/core/guards/role.guard.ts`

---

## 6. Routing

Three new routes added to `app.routes.ts` (all lazy-loaded):

```typescript
{
  path: 'patients',
  loadComponent: () =>
    import('./features/patients/patient-search.component').then(m => m.PatientSearchComponent),
  canActivate: [authGuard, roleGuard('Receptionist', 'ITAdmin')],
},
{
  path: 'patients/new',
  loadComponent: () =>
    import('./features/patients/register-patient.component').then(m => m.RegisterPatientComponent),
  canActivate: [authGuard, roleGuard('Receptionist', 'ITAdmin')],
},
{
  path: 'patients/:id',
  loadComponent: () =>
    import('./features/patients/patient-detail.component').then(m => m.PatientDetailComponent),
  canActivate: [authGuard, roleGuard('Receptionist', 'ITAdmin')],
},
```

> `patients/new` must appear **before** `patients/:id` in the route array to prevent `:id` from capturing the literal string `"new"`.

### Shell Navigation

Add "Patients" nav link in `shell.component.html` visible to Receptionist and ITAdmin:
```html
@if (user().role === 'Receptionist' || user().role === 'ITAdmin') {
  <a routerLink="/patients" data-testid="nav-patients">Patients</a>
}
```

---

## 7. HIPAA Compliance

| Rule | Implementation |
|---|---|
| No PHI in URL path | PatientId (GUID) in path is not PHI. Search term goes in `?search=` query param, not in path |
| No PHI in `console.log` | No patient names, DOBs, or document IDs may appear in any `console.*` call |
| Edit disabled for Anonymized | `PatientFormComponent` rendered with `disabled` fields when `status === 'Anonymized'`; edit button hidden |
| Sentinel values shown as-is | `"ANONYMIZED"` sentinel values from the API are displayed directly — no client-side masking |
| `hipaa.spec.ts` coverage | Add assertions verifying no patient PII appears in console output during patient workflows |

---

## 8. Testing

### 8.1 Unit Tests (`patients.service.spec.ts`)

| Test | Asserts |
|---|---|
| `search() updates searchResults signal` | Adapter `search()` called with term; `searchResults()` updated with returned array |
| `register() returns new patient id` | Adapter `register()` called with params; resolved id returned |
| `update() calls adapter with id and params` | Adapter `update()` called with correct id and params |
| `anonymize() calls adapter with id` | Adapter `anonymize()` called with correct id |
| `error signal set on adapter rejection` | `error()` signal set to message when adapter throws `ApiError` |

### 8.2 Integration Tests

**`patient-search.component.integration.spec.ts`**

| Scenario | Assertions |
|---|---|
| Search with results | Table renders one row per `PatientSearchResult`; Full Name visible in each row |
| Search with no results | `patient-search-empty-state` element is visible |
| Register button navigates | Clicking `register-patient-btn` navigates to `/patients/new` |

**`patient-detail.component.integration.spec.ts`**

| Scenario | Assertions |
|---|---|
| Active patient — Receptionist role | `patient-edit-btn` visible; `patient-anonymize-btn` absent |
| Active patient — ITAdmin role | Both `patient-edit-btn` and `patient-anonymize-btn` visible |
| Anonymized patient — any role | `patient-edit-btn` absent; `patient-anonymize-btn` absent or disabled; `patient-status-badge` reads "Anonymized" |
| Anonymize confirmation flow | Click `patient-anonymize-btn` → `anonymize-confirm-btn` appears; click confirm → service `anonymize()` called |

### 8.3 E2E Tests (`e2e/patients.spec.ts`)

| Spec | Role | Flow |
|---|---|---|
| **Full register + edit workflow** | Receptionist | Search by a unique name → no results → click Register → fill form → submit → navigate to detail → verify name displayed → click Edit → change name → submit → verify updated name |
| **Anonymize workflow** | ITAdmin | Navigate to `/patients` → search → click result → click Anonymize → confirm → verify "Anonymized" badge; Edit button gone; Anonymize button gone |
| **Role guard — LabTechnician** | LabTechnician | Navigate to `/patients` → redirected to `/unauthorized` |
| **Role guard — Physician** | Physician | Navigate to `/patients` → redirected to `/unauthorized` |

All specs use `waitForResponse()` before asserting state that depends on an API call completing.

---

## 9. Files Overview

### Backend (new)

```
src/HC.LIS/HC.LIS.API/Modules/PatientManagement/
  PatientManagementAutofacModule.cs
  Patients/
    PatientsEndpoints.cs
    RegisterPatient/
      RegisterPatientRequest.cs
      RegisterPatientEndpoint.cs
    SearchPatients/
      SearchPatientsEndpoint.cs
    GetPatientDetails/
      GetPatientDetailsEndpoint.cs
    UpdatePatient/
      UpdatePatientRequest.cs
      UpdatePatientEndpoint.cs
    AnonymizePatient/
      AnonymizePatientEndpoint.cs
```

### Backend (modified)

```
src/HC.LIS/HC.LIS.API/Program.cs  — add Autofac module, Initialize(), policy, route group
```

### Frontend (new)

```
src/app/core/domain/
  patient-details.ts
  patient-search-result.ts
  register-patient-params.ts

src/app/core/application/
  i-patients-port.ts
  patients.service.ts
  patients.service.spec.ts

src/app/core/infrastructure/patients/
  sdk-patients-adapter.ts

src/app/features/patients/
  patient-search.component.ts
  patient-search.component.html
  patient-search.component.css
  patient-search.component.integration.spec.ts
  register-patient.component.ts
  register-patient.component.html
  register-patient.component.css
  patient-form.component.ts
  patient-form.component.html
  patient-form.component.css
  patient-detail.component.ts
  patient-detail.component.html
  patient-detail.component.css
  patient-detail.component.integration.spec.ts

e2e/patients.spec.ts
```

### Frontend (modified)

```
src/app/core/guards/role.guard.ts           — extend to rest params
src/app/app.routes.ts                       — add 3 patient routes
src/app/core/shell/shell.component.html     — add Patients nav link
src/app/app.config.ts                       — provide PATIENTS_PORT token
e2e/hipaa.spec.ts                           — add patient PHI assertions
```

---

## 10. Open Design Decisions

| # | Decision | Recommendation |
|---|---|---|
| 1 | Search debounce vs. explicit submit | Debounce 300 ms (live search) — low API cost; matches receptionist UX pattern of typing a partial name |
| 2 | Edit in-place vs. separate `/patients/:id/edit` route | Inline form toggle — avoids a fourth route; detail and edit share the same loaded `patient` signal |
| 3 | Anonymize confirmation UX | Two-click inline confirm (no modal) — avoids modal library dependency; revisit when Angular Material is adopted |
| 4 | Anonymized patients in search results | Displayed with sentinel values + "Anonymized" badge — consistent with backend behavior; no client-side filtering |
| 5 | `orders/new` patient ID field | Not changed in this release; future iteration replaces the free-text patient ID input with a `/patients` search picker |
| 6 | Search minimum character threshold | No minimum enforced client-side (backend handles empty/short terms); revisit if search performance degrades |
