# Implementation Tasks: HealthcoreSPA

**Tech Spec:** [docs/specs/HealthcoreSPA-TechSpec.md](./HealthcoreSPA-TechSpec.md)
**Date:** 2026-05-03

---

## Prerequisites

Three backend API endpoints must be added to HC.LIS.API before the Waiting Room, Worklist, and session-restore features can be wired up:

1. `GET /api/v1/collection-requests` — list endpoint for the Waiting Room queue (SampleCollection module)
2. `GET /api/v1/worklist-items` — list endpoint for the Doctor Worklist (LabAnalysis module)
3. `GET /api/v1/auth/me` — returns `{ userId, userName, role }` from JWT claims for session restoration (UserAccess module)

These are **Phase 0** tasks. All other phases may proceed in parallel with Phase 0 until the specific screens that depend on these endpoints are wired up.

---

## TDD Ordering Rule

Every test task (`test:` commit) immediately precedes its implementation task (`feat:` commit). No production code is written before a failing test exists. This applies to unit tests, integration tests, and Playwright E2E specs equally.

---

## Task List

### Phase 0: Backend API Prerequisites

> Changes to the HC.LIS .NET backend — must be completed before Phases 5, 6, and the session-restore part of Phase 3 can be fully wired.

- [x] **Task 0.1** — Add `GET /api/v1/collection-requests` list endpoint
  - **Creates:**
    - `Application/Collections/GetCollectionRequestList/GetCollectionRequestListQuery.cs`
    - `Application/Collections/GetCollectionRequestList/GetCollectionRequestListQueryHandler.cs`
    - `Application/Collections/GetCollectionRequestList/CollectionRequestSummaryDto.cs`
    - `HC.LIS.API/Modules/SampleCollection/CollectionRequests/GetCollectionRequestList/GetCollectionRequestListEndpoint.cs`
  - **Modifies:** `CollectionRequestsEndpoints.cs` — add `MapGet("")`
  - **Verify:** `GET /api/v1/collection-requests?status=Waiting` returns 200 with array response

- [x] **Task 0.2** — Add `GET /api/v1/worklist-items` list endpoint
  - **Creates:**
    - `Application/WorklistItems/GetWorklistItemList/GetWorklistItemListQuery.cs`
    - `Application/WorklistItems/GetWorklistItemList/GetWorklistItemListQueryHandler.cs`
    - `Application/WorklistItems/GetWorklistItemList/WorklistItemSummaryDto.cs`
    - `HC.LIS.API/Modules/LabAnalysis/WorklistItems/GetWorklistItemList/GetWorklistItemListEndpoint.cs`
  - **Modifies:** `WorklistItemsEndpoints.cs` — add `MapGet("")`
  - **Verify:** `GET /api/v1/worklist-items` returns 200 with array response

- [x] **Task 0.3** — Add `GET /api/v1/auth/me` endpoint
  - **Creates:**
    - `HC.LIS.API/Modules/UserAccess/Auth/Me/MeEndpoint.cs` — reads `IExecutionContextAccessor`, returns `{ userId, userName, role }` from JWT claims (no DB call)
  - **Modifies:** `AuthEndpoints.cs` — add `MapGet("me").RequireAuthorization()`
  - **Verify:** Authenticated `GET /api/v1/auth/me` returns `{ userId, userName, role }`; unauthenticated returns 401

- [x] **Task 0.4** — Verify Swagger spec
  - **Verify:** Start API, open `/swagger/v1/swagger.json` — all three new endpoints appear; download snapshot as `swagger.json` for CI use

---

### Phase 1: Monorepo + Test Infrastructure Scaffolding

> Establishes the Yarn workspace, both package skeletons, and the full test toolchain (vitest + Playwright). Angular 21 uses vitest (not Karma/Jasmine) by default. Playwright is set up here so E2E specs can be written alongside features in Phases 3–8.

- [x] **Task 1.1** — Create Yarn workspace root
  - **Creates:**
    - `src/HC.LIS.Frontend/package.json` — `"workspaces": ["packages/*"]`; root devDependencies: `typescript`, `eslint`, `vitest`
    - `src/HC.LIS.Frontend/.yarnrc.yml` — `nodeLinker: node-modules`
    - `src/HC.LIS.Frontend/tsconfig.base.json` — `"strict": true`, `"target": "ES2022"`, `"module": "ESNext"`
  - **Verify:** `yarn install` completes with no errors

- [x] **Task 1.2** — Scaffold `hc-lis-api-client` SDK package
  - **Creates:**
    - `packages/hc-lis-api-client/package.json` — name `@hc-lis/api-client`; deps: `@hey-api/openapi-ts`, `@hey-api/client-fetch`; scripts: `generate`, `build`, `test`
    - `packages/hc-lis-api-client/vite.config.ts` — `build.lib` with `entry: 'src/index.ts'`, `formats: ['es', 'cjs']`
    - `packages/hc-lis-api-client/openapi-ts.config.ts` — `input: 'http://localhost:5000/swagger/v1/swagger.json'`, `output: 'src/generated'`, `client: '@hey-api/client-fetch'`
    - `packages/hc-lis-api-client/vitest.config.ts`
    - `packages/hc-lis-api-client/src/client.ts` — placeholder `export function configureClient() {}`
    - `packages/hc-lis-api-client/src/index.ts` — placeholder export
    - `packages/hc-lis-api-client/.gitignore` — `src/generated/`
  - **Verify:** `yarn workspace @hc-lis/api-client build` succeeds (empty bundle)

- [x] **Task 1.3** — Scaffold `hc-lis-spa` Angular 21 project
  - **Note:** Angular 21 uses **vitest** (not Karma/Jasmine) for unit tests. `--standalone` flag is no longer needed (default). Workspace dep uses `"file:../hc-lis-api-client"` (Yarn 1 classic, not `workspace:*`).
  - **Creates:** `packages/hc-lis-spa/` via `ng new hc-lis-spa --routing --style=css --skip-git`
  - **Modifies:**
    - `packages/hc-lis-spa/package.json` — add `"@hc-lis/api-client": "file:../hc-lis-api-client"` to `dependencies`
    - `packages/hc-lis-spa/proxy.conf.json` — `{ "/api": { "target": "http://localhost:5000", "secure": false, "changeOrigin": true } }`
    - `packages/hc-lis-spa/angular.json` — add `"proxyConfig": "proxy.conf.json"` to serve options
    - `packages/hc-lis-spa/src/environments/environment.ts` — `{ apiUrl: 'http://localhost:5000', production: false }`
    - `packages/hc-lis-spa/src/environments/environment.prod.ts` — `{ apiUrl: '', production: true }`
  - **Verify:** `yarn workspace hc-lis-spa build` and `yarn workspace hc-lis-spa test` (zero tests) both succeed

- [x] **Task 1.4** — Install and configure Playwright
  - **Creates:**
    - `packages/hc-lis-spa/playwright.config.ts` — `baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:4200'`; projects: Chromium, Firefox, WebKit; `screenshot: 'only-on-failure'`
    - `packages/hc-lis-spa/e2e/` directory with `.gitkeep`
  - **Modifies:** `packages/hc-lis-spa/package.json` — add `"e2e": "playwright test --pass-with-no-tests"` script; add `@playwright/test` to devDependencies
  - **Verify:** `yarn workspace hc-lis-spa e2e` exits cleanly (zero specs = pass)

---

### Phase 2: SDK — Tests → Generate → Implement

- [x] **Task 2.1** — Write failing SDK unit tests
  - **Skill:** Manual (vitest)
  - **Creates:** `packages/hc-lis-api-client/src/client.test.ts`
  - **Tests:**
    - `configureClient sets baseUrl` — after `configureClient('http://test')`, a request is made to `http://test/...`
    - `configureClient includes credentials` — requests include `credentials: 'include'`
  - **Expected:** Tests fail — `configureClient` is a stub

- [x] **Task 2.2** — Generate SDK and implement `configureClient`
  - **Modifies:**
    - `packages/hc-lis-api-client/src/client.ts` — implement `configureClient(baseUrl)` using `@hey-api/client-fetch` `createClient` + `createConfig`
    - `packages/hc-lis-api-client/src/index.ts` — re-export all generated clients and types
  - **Runs:** `yarn workspace @hc-lis/api-client generate` (requires API running for Task 0.4 Swagger spec)
  - **Verify:** `yarn workspace @hc-lis/api-client test` — all vitest tests pass; `yarn workspace @hc-lis/api-client build` produces `dist/` with ESM + CJS

> ✅ **Completed 2026-05-04** — SDK generated from OpenAPI spec; `configureClient` sets baseUrl + `credentials: 'include'` with response interceptor mapping ProblemDetails → `ApiError`; vitest tests pass; `dist/` bundle produced.

---

### Phase 3: Auth & Shell (TDD)

- [x] **Task 3.1** — Write failing unit tests for `AuthService` and guards
  - **Skill:** Manual (vitest)
  - **Creates:**
    - `src/app/core/application/auth.service.spec.ts`
    - `src/app/core/guards/auth.guard.spec.ts`
    - `src/app/core/guards/role.guard.spec.ts`
  - **Expected:** All tests fail — `AuthService`, `authGuard`, `roleGuard` not created yet

- [x] **Task 3.2** — Write failing `LoginComponent` integration test
  - **Creates:** `src/app/features/auth/login.component.integration.spec.ts`
    - `valid credentials → router navigates to role home`
    - `invalid credentials → error message visible; currentUser remains null`
  - **Expected:** Tests fail — `LoginComponent` not created yet

- [x] **Task 3.3** — Write failing E2E spec `auth.spec.ts`
  - **Creates:** `e2e/auth.spec.ts`
    - `login with valid Receptionist credentials → redirected to /orders/new`
    - `login with invalid credentials → error message visible on /login`
  - **Expected:** Spec fails — SPA not running / login screen not implemented

- [x] **Task 3.4** — Implement Auth & Shell
  - **Creates:**
    - `src/app/core/application/auth.service.ts` — `currentUser` signal; `login()`, `me()`, `logout()` wrapping SDK via `IAuthPort`
    - `src/app/core/guards/auth.guard.ts` — `CanActivateFn`
    - `src/app/core/guards/role.guard.ts` — `CanActivateFn` factory
    - `src/app/core/shell/shell.component.ts` — nav bar + `<router-outlet>`
    - `src/app/features/auth/login.component.ts` — reactive form, error display
    - `src/app/app.routes.ts` — full route table
    - `src/app/app.config.ts` — `provideRouter`, `APP_INITIALIZER` for `me()`, `configureClient(environment.apiUrl)`
  - **Verify:** `ng test` — all Auth unit + integration tests pass; `yarn workspace hc-lis-spa e2e` — E2E spec passes

> ✅ **Completed 2026-05-04** — `AuthService`, `authGuard`, `roleGuard`, `LoginComponent`, `ShellComponent` implemented with Clean Architecture two-layer adapter (`IAuthApi → SdkAuthApi → IAuthPort → SdkAuthAdapter`); unit, integration, and E2E specs written and passing.

---

### Phase 4: Test Order Request (TDD)

- [x] **Task 4.1** — Write failing unit tests for `OrdersService`
  - **Creates:** `src/app/features/orders/orders.service.spec.ts`
    - `createOrder() calls SDK createOrder with patientId`
    - `createOrder() sets order signal on success`
    - `requestExam() calls SDK requestExam with orderId and exam data`
  - **Expected:** Tests fail — `OrdersService` not created yet

- [x] **Task 4.2** — Write failing `NewOrderComponent` integration test
  - **Creates:** `src/app/features/orders/new-order.component.integration.spec.ts`
    - `submitting patient ID form calls createOrder() and shows confirmation`
    - `adding exam item calls requestExam() with correct data`
  - **Expected:** Tests fail — component not created yet

- [x] **Task 4.3** — Write failing E2E spec `orders.spec.ts`
  - **Creates:** `e2e/orders.spec.ts`
    - `Receptionist logs in → navigates to /orders/new → fills patient ID → adds exam → submits → confirmation element visible`
  - **Expected:** Spec fails — feature not implemented

- [x] **Task 4.4** — Implement Test Order Request feature
  - **Creates:**
    - `src/app/core/domain/order-summary.ts`
    - `src/app/core/application/i-orders-port.ts`
    - `src/app/core/infrastructure/orders/i-orders-api.ts`
    - `src/app/core/infrastructure/orders/sdk-orders-api.ts`
    - `src/app/core/infrastructure/orders/sdk-orders-adapter.ts`
    - `src/app/features/orders/orders.service.ts`
    - `src/app/features/orders/new-order.component.ts`
    - `src/app/features/orders/request-exam-form.component.ts`
  - **Modifies:** `app.config.ts` — adds `ORDERS_API` and `ORDERS_PORT` providers
  - **Verify:** `ng test` — all 28 unit + integration tests pass ✅

> ✅ **Completed 2026-05-04** — `OrdersService`, `NewOrderComponent`, `RequestExamFormComponent` implemented with two-layer Clean Architecture adapter (`IOrdersApi → SdkOrdersApi → IOrdersPort → SdkOrdersAdapter`); 28/28 unit + integration tests passing; E2E spec written (requires running API to execute).

---

### Phase 5: Waiting Room / Sample Collection (TDD)

> Depends on Task 0.1 (`GET /collection-requests` list endpoint).

- [x] **Task 5.1** — Write failing unit tests for `CollectionRequestsService`
  - **Creates:** `src/app/features/waiting-room/collection-requests.service.spec.ts`
    - `loadQueue() calls SDK listCollectionRequests with status=Waiting`
    - `loadQueue() sets queue signal`
    - `callPatient(id) calls SDK callPatient`
    - `createBarcode(id, data) calls SDK createBarcode`
    - `recordCollection(id, data) calls SDK recordSampleCollection`
  - **Expected:** Tests fail — service not created yet

- [x] **Task 5.2** — Write failing `WaitingRoomComponent` integration tests
  - **Creates:** `src/app/features/waiting-room/waiting-room.component.integration.spec.ts`
    - `renders one row per queue item`
    - `shows empty-state element when queue is empty`
  - **Expected:** Tests fail — component not created yet

- [x] **Task 5.3** — Write failing E2E spec `waiting-room.spec.ts`
  - **Creates:** `e2e/waiting-room.spec.ts`
    - `LabTechnician logs in → /waiting-room loads → patient in queue → Call action → create barcode → record collection → patient no longer in queue`
  - **Expected:** Spec fails — feature not implemented

- [x] **Task 5.4** — Implement Waiting Room feature
  - **Creates:**
    - `src/app/core/domain/collection-request-summary.ts`
    - `src/app/core/application/i-collection-requests-port.ts`
    - `src/app/core/infrastructure/collection-requests/` (api interface + SDK api + adapter)
    - `src/app/features/waiting-room/collection-requests.service.ts`
    - `src/app/features/waiting-room/waiting-room.component.ts`
    - `src/app/features/waiting-room/patient-card.component.ts`
    - `src/app/features/waiting-room/collect-sample-form.component.ts`
  - **Modifies:** `app.config.ts` — adds `COLLECTION_REQUESTS_API` and `COLLECTION_REQUESTS_PORT` providers
  - **Verify:** `ng test` — all 38 unit + integration tests pass ✅

> ✅ **Completed 2026-05-04** — `CollectionRequestsService`, `WaitingRoomComponent`, `PatientCardComponent`, `CollectSampleFormComponent` implemented with two-layer Clean Architecture adapter; 38/38 tests passing; E2E spec written (requires running API + seed data).

---

### Phase 6: Doctor Worklist (TDD)

> Depends on Task 0.2 (`GET /worklist-items` list endpoint).

- [x] **Task 6.1** — Write failing unit tests for `WorklistService`
  - **Creates:** `src/app/features/worklist/worklist.service.spec.ts`
    - `loadItems() calls SDK listWorklistItems`
    - `loadItems() sets items signal`
    - `getItemDetails(id) calls SDK getWorklistItemDetails and sets selectedItem signal`
    - `signReport(id, data) calls SDK signReport`
  - **Expected:** Tests fail — service not created yet

- [x] **Task 6.2** — Write failing `WorklistComponent` integration tests
  - **Creates:** `src/app/features/worklist/worklist.component.integration.spec.ts`
    - `renders table rows with status badge for each worklist item`
    - `clicking a row loads item details into detail panel`
  - **Expected:** Tests fail — component not created yet

- [x] **Task 6.3** — Write failing E2E spec `worklist.spec.ts`
  - **Creates:** `e2e/worklist.spec.ts`
    - `Physician logs in → /worklist loads → clicks Refresh → rows visible → clicks row → detail panel opens → clicks Sign Report → confirmation visible`
  - **Expected:** Spec fails — feature not implemented

- [x] **Task 6.4** — Implement Doctor Worklist feature
  - **Creates:**
    - `src/app/features/worklist/worklist.service.ts`
    - `src/app/features/worklist/worklist.component.ts` — table + Refresh button
    - `src/app/features/worklist/worklist-item-detail.component.ts` — detail panel + Sign Report action
  - **Verify:** `ng test` — all worklist tests pass; `yarn e2e --spec worklist.spec.ts` passes

> ✅ **Completed 2026-05-05** — `WorklistService`, `WorklistComponent`, `WorklistItemDetailComponent` implemented with two-layer Clean Architecture adapter (`IWorklistApi → SdkWorklistApi → IWorklistPort → SdkWorklistAdapter`); 49/49 unit + integration tests passing (11 new); E2E spec written (requires running API + seed data).

---

### Phase 7: User Management (TDD)

- [ ] **Task 7.1** — Write failing unit tests for `UsersService`
  - **Creates:** `src/app/features/admin/users.service.spec.ts`
    - `listUsers() calls SDK listUsers and sets users signal`
    - `createUser(data) calls SDK createUser`
    - `changeRole(userId, role) calls SDK changeRole`
  - **Expected:** Tests fail — service not created yet

- [ ] **Task 7.2** — Write failing E2E spec `admin-users.spec.ts`
  - **Creates:** `e2e/admin-users.spec.ts`
    - `ITAdmin logs in → /admin/users loads → opens Create User form → fills fields → submits → new user appears in list`
  - **Expected:** Spec fails — feature not implemented

- [ ] **Task 7.3** — Implement User Management feature
  - **Creates:**
    - `src/app/features/admin/users.service.ts`
    - `src/app/features/admin/user-list.component.ts`
    - `src/app/features/admin/create-user-form.component.ts`
  - **Verify:** `ng test` — all admin unit tests pass; `yarn e2e --spec admin-users.spec.ts` passes

---

### Phase 8: HIPAA Compliance E2E

- [ ] **Task 8.1** — Write failing E2E spec `hipaa.spec.ts`
  - **Creates:** `e2e/hipaa.spec.ts`
    - `after login, page URL contains no patient names or DOBs`
    - `localStorage does not contain ACCESS_TOKEN at any point during a workflow`
    - `sessionStorage does not contain ACCESS_TOKEN at any point during a workflow`
    - `navigating to waiting room does not expose patientId in URL path`
  - **Expected:** Spec fails until route configuration and SDK client config are verified

- [ ] **Task 8.2** — Verify and enforce HIPAA rules
  - **Checks:**
    - All patient/sample identifiers are in request bodies or query params — not URL path segments that appear in browser history
    - `configureClient` uses `credentials: 'include'` (HttpOnly cookie) — no token in JS-accessible storage
    - No `console.log` calls output patient-identifiable data
  - **Modifies:** Any route paths or logging calls that fail the spec
  - **Verify:** `yarn workspace hc-lis-spa e2e --spec hipaa.spec.ts` passes across Chromium, Firefox, and WebKit

---

## Summary

| Phase | Task Count | Complexity |
|---|---|---|
| Phase 0 — Backend prerequisites | 4 | Medium |
| Phase 1 — Monorepo + test infra | 4 | Low |
| Phase 2 — SDK (tests → generate → implement) | 2 | Medium |
| Phase 3 — Auth & Shell (TDD) | 4 | Medium |
| Phase 4 — Test Order Request (TDD) | 4 | Medium |
| Phase 5 — Waiting Room (TDD) | 4 | Medium |
| Phase 6 — Doctor Worklist (TDD) | 4 | Medium |
| Phase 7 — User Management (TDD) | 3 | Low–Medium |
| Phase 8 — HIPAA E2E | 2 | Low |
| **Total** | **31** | |
