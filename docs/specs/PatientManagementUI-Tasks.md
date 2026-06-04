# Implementation Tasks: PatientManagement UI

**Tech Spec:** [docs/specs/PatientManagementUI-TechSpec.md](./PatientManagementUI-TechSpec.md)
**Date:** 2026-06-02

---

## TDD Ordering Rule

Every test task (`test:` commit) immediately precedes its implementation task (`feat:` commit). No production code is written before a failing test exists. This applies to unit tests, integration tests, and Playwright E2E specs equally.

---

## Task List

### Phase 0: Backend API

> Changes to `HC.LIS.API` and `HC.LIS.API.Modules.PatientManagement`. All subsequent phases depend on Phase 0 completing first (SDK regeneration in Phase 1 requires Swagger to be up to date).

- [x] **Task 0.1** — Add `PatientManagementAutofacModule` and wire `Program.cs`
  - **Creates:**
    - `src/HC.LIS/HC.LIS.API/Modules/PatientManagement/PatientManagementAutofacModule.cs`
  - **Modifies:** `Program.cs`
    - Add `containerBuilder.RegisterModule(new PatientManagementAutofacModule())`
    - Add `PatientManagementStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null)`
    - Add `options.AddPolicy("PatientManagement", policy => policy.RequireRole("Receptionist", "ITAdmin"))` inside `AddAuthorization()`
    - Add `v1.MapGroup("patients").MapPatientsEndpoints()`
  - **Verify:** API starts without errors; Swagger UI shows a "Patients" section

- [x] **Task 0.2** — Add `POST /api/v1/patients` — Register Patient
  - **Creates:**
    - `Modules/PatientManagement/Patients/RegisterPatient/RegisterPatientRequest.cs` — `FullName`, `DateOfBirth`, `Gender?`, `MothersFullName?`, `DocumentId?`, `Phone?`, `Email?`
    - `Modules/PatientManagement/Patients/RegisterPatient/RegisterPatientEndpoint.cs` — `Guid.CreateVersion7()` for id, `SystemClock.Now` for `RegisteredAt`; returns `TypedResults.Created`
    - `Modules/PatientManagement/Patients/PatientsEndpoints.cs` — `MapPatientsEndpoints()` with first `MapPost` entry
  - **Verify:** `POST /api/v1/patients` with valid body returns `201` with `{ id: Guid }` and `Location` header

- [x] **Task 0.3** — Add `GET /api/v1/patients?search={term}` — Search Patients
  - **Creates:**
    - `Modules/PatientManagement/Patients/SearchPatients/SearchPatientsEndpoint.cs` — binds `string search` from query; wraps term with `%` wildcards; calls `SearchPatientsQuery`
  - **Modifies:** `PatientsEndpoints.cs` — add `MapGet("")` entry
  - **Verify:** `GET /api/v1/patients?search=John` returns `200` with array of `PatientSearchResultDto`

- [x] **Task 0.4** — Add `GET /api/v1/patients/{id:guid}` — Get Patient Details
  - **Creates:**
    - `Modules/PatientManagement/Patients/GetPatientDetails/GetPatientDetailsEndpoint.cs` — calls `GetPatientDetailsQuery`; returns `404` if null
  - **Modifies:** `PatientsEndpoints.cs` — add `MapGet("{id:guid}")` entry
  - **Verify:** `GET /api/v1/patients/{id}` for a registered patient returns `200` with `PatientDetailsDto`; unknown id returns `404`

- [x] **Task 0.5** — Add `PUT /api/v1/patients/{id:guid}` — Update Patient
  - **Creates:**
    - `Modules/PatientManagement/Patients/UpdatePatient/UpdatePatientRequest.cs`
    - `Modules/PatientManagement/Patients/UpdatePatient/UpdatePatientEndpoint.cs` — `SystemClock.Now` for `UpdatedAt`; returns `TypedResults.NoContent()`
  - **Modifies:** `PatientsEndpoints.cs` — add `MapPut("{id:guid}")` entry
  - **Verify:** `PUT /api/v1/patients/{id}` for an Active patient returns `204`; for an Anonymized patient returns `400`

- [x] **Task 0.6** — Add `POST /api/v1/patients/{id:guid}/anonymize` — Anonymize Patient
  - **Creates:**
    - `Modules/PatientManagement/Patients/AnonymizePatient/AnonymizePatientEndpoint.cs` — `SystemClock.Now` for `AnonymizedAt`; `.RequireAuthorization("ITAdmin")`; returns `TypedResults.NoContent()`
  - **Modifies:** `PatientsEndpoints.cs` — add `MapPost("{id:guid}/anonymize")` entry
  - **Verify:** ITAdmin `POST /api/v1/patients/{id}/anonymize` returns `204`; second call returns `400`; Receptionist call returns `403`

- [x] **Task 0.7** — Verify Swagger spec and regenerate SDK
  - **Verify:** Start API; open `/swagger/v1/swagger.json` — all 5 patient endpoints present with correct request/response schemas
  - **Action:** Run `yarn workspace @hc-lis/api-client generate` to regenerate the SDK; verify `src/generated/` contains patient service functions

---

### Phase 1: Angular Domain + Application + Infrastructure

> Pure TypeScript layer — no Angular components. Can proceed in parallel with Phase 0's API work, but SDK adapter (Task 1.4) requires the regenerated SDK from Task 0.7.

- [x] **Task 1.1** — Add domain interfaces
  - **Creates:**
    - `src/app/core/domain/patient-details.ts` — `PatientDetails` interface
    - `src/app/core/domain/patient-search-result.ts` — `PatientSearchResult` interface
    - `src/app/core/domain/register-patient-params.ts` — `RegisterPatientParams` and `UpdatePatientParams` types
  - **Verify:** `tsc --noEmit` passes with zero errors

- [x] **Task 1.2** — Add port interface
  - **Creates:**
    - `src/app/core/application/i-patients-port.ts` — `IPatientsPort` interface + `PATIENTS_PORT` injection token
  - **Verify:** `tsc --noEmit` passes

- [x] **Task 1.3 (test)** — Write unit tests for `PatientsService`
  - **Creates:** `src/app/core/application/patients.service.spec.ts` — 5 failing tests (see TechSpec §8.1)
  - **Verify:** Tests fail (service not implemented yet)

- [x] **Task 1.4 (feat)** — Implement `PatientsService`
  - **Creates:** `src/app/core/application/patients.service.ts` — signals + methods; injects `PATIENTS_PORT`
  - **Verify:** All 5 unit tests in Task 1.3 pass

- [x] **Task 1.5** — Add SDK infrastructure adapter
  - **Creates:** `src/app/core/infrastructure/patients/sdk-patients-adapter.ts` — implements `IPatientsPort`; calls generated SDK functions
  - **Modifies:** `src/app/app.config.ts` — add `{ provide: PATIENTS_PORT, useClass: SdkPatientsAdapter }` to providers
  - **Verify:** `tsc --noEmit` passes; `yarn workspace hc-lis-spa test` passes

---

### Phase 2: Guard Extension + Routing + Shell Navigation

- [x] **Task 2.1** — Extend `roleGuard` to accept rest params
  - **Modifies:** `src/app/core/guards/role.guard.ts` — change `role: UserRole` to `...roles: UserRole[]`; update `includes()` check
  - **Verify:** Existing E2E specs still pass (`auth.spec.ts`, `admin-users.spec.ts`); `tsc --noEmit` passes

- [x] **Task 2.2** — Add patient routes and shell nav link
  - **Modifies:** `src/app/app.routes.ts` — add 3 patient routes (search before detail to avoid `:id` capturing `"new"`)
  - **Modifies:** `src/app/core/shell/shell.component.html` — add Patients nav link with `data-testid="nav-patients"` gated on Receptionist + ITAdmin role
  - **Verify:** Navigating to `/patients` while logged in as Receptionist loads without a routing error; Physician is redirected to `/unauthorized`

---

### Phase 3: Patient Search Screen

- [ ] **Task 3.1 (test)** — Write integration tests for `PatientSearchComponent`
  - **Creates:** `src/app/features/patients/patient-search.component.integration.spec.ts` — 3 failing tests (see TechSpec §8.2)
  - **Verify:** Tests fail (component not implemented yet)

- [ ] **Task 3.2 (feat)** — Implement `PatientSearchComponent`
  - **Creates:**
    - `src/app/features/patients/patient-search.component.ts` — debounced search input, result table, empty state; injects `PatientsService` and `Router`
    - `src/app/features/patients/patient-search.component.html`
    - `src/app/features/patients/patient-search.component.css`
  - **Verify:** All 3 integration tests in Task 3.1 pass; `yarn workspace hc-lis-spa test` passes

---

### Phase 4: Register Patient Screen

- [ ] **Task 4.1 (feat)** — Implement `PatientFormComponent`
  - **Creates:**
    - `src/app/features/patients/patient-form.component.ts` — reactive form; `@Input() initialValues`; `@Output() formSubmit`
    - `src/app/features/patients/patient-form.component.html` — all 7 fields with `data-testid` attributes
    - `src/app/features/patients/patient-form.component.css`
  - **Verify:** `tsc --noEmit` passes; form renders with all required `data-testid` attributes present

- [ ] **Task 4.2 (test)** — Write integration test for `RegisterPatientComponent`
  - **Creates:** integration test block in `patient-search.component.integration.spec.ts` or a new `register-patient.component.integration.spec.ts` — covers form submit navigates to detail (see TechSpec §8.2)
  - **Verify:** Test fails (component not implemented yet)

- [ ] **Task 4.3 (feat)** — Implement `RegisterPatientComponent`
  - **Creates:**
    - `src/app/features/patients/register-patient.component.ts` — embeds `PatientFormComponent`; on `formSubmit` calls `PatientsService.register()` then navigates to `/patients/:newId`
    - `src/app/features/patients/register-patient.component.html`
    - `src/app/features/patients/register-patient.component.css`
  - **Verify:** Integration test from Task 4.2 passes; `yarn workspace hc-lis-spa test` passes

---

### Phase 5: Patient Detail Screen

- [ ] **Task 5.1 (test)** — Write integration tests for `PatientDetailComponent`
  - **Creates:** `src/app/features/patients/patient-detail.component.integration.spec.ts` — 4 failing tests (see TechSpec §8.2)
  - **Verify:** Tests fail (component not implemented yet)

- [ ] **Task 5.2 (feat)** — Implement `PatientDetailComponent`
  - **Creates:**
    - `src/app/features/patients/patient-detail.component.ts` — loads patient on init; status badge; edit form toggle; anonymize with confirm step; ITAdmin-only visibility guard using `AuthService.currentUser()`
    - `src/app/features/patients/patient-detail.component.html`
    - `src/app/features/patients/patient-detail.component.css`
  - **Verify:** All 4 integration tests in Task 5.1 pass; `yarn workspace hc-lis-spa test` passes

---

### Phase 6: Playwright E2E

- [ ] **Task 6.1 (test)** — Write `patients.spec.ts` — all 4 flows failing
  - **Creates:** `src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/patients.spec.ts` — full register + edit workflow, anonymize workflow, LabTech role guard, Physician role guard (see TechSpec §8.3)
  - **Verify:** All 4 tests fail (as expected — screens not wired to real API yet in E2E)

- [ ] **Task 6.2** — Wire E2E environment and verify all flows pass
  - **Requires:** Backend API running with seed data; Tasks 0.1–0.7, 3.2, 4.3, 5.2 complete
  - **Verify:** `yarn workspace hc-lis-spa e2e --grep patients` — all 4 flows green across Chromium, Firefox, WebKit

---

### Phase 7: HIPAA

- [ ] **Task 7.1** — Add patient PHI assertions to `hipaa.spec.ts`
  - **Modifies:** `e2e/hipaa.spec.ts` — add assertions that patient `FullName`, `DateOfBirth`, and `DocumentId` values do not appear in browser console output during a patient search or detail load
  - **Verify:** `yarn workspace hc-lis-spa e2e --grep hipaa` passes
