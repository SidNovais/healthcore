# Technical Spec: HealthcoreSPA

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-05-03
**PRD Reference:** [docs/prd/HealthcoreSPA.md](../prd/HealthcoreSPA.md)

---

## 1. Overview

HealthcoreSPA is a browser-only Single Page Application that surfaces every HC.LIS workflow to non-technical lab staff through a role-scoped, HIPAA-compliant UI. It lives in a Yarn workspaces monorepo at `src/HC.LIS.Frontend/` alongside the existing .NET backend.

**Technology:** Angular 17+ (standalone components), TypeScript, Yarn workspaces, Vite, Playwright

**Monorepo packages:**

| Package | Path | Purpose |
|---|---|---|
| `hc-lis-api-client` | `packages/hc-lis-api-client/` | TypeScript SDK — auto-generated from HC.LIS.API OpenAPI spec; Vite library mode |
| `hc-lis-spa` | `packages/hc-lis-spa/` | Angular 17+ SPA — consumes the SDK; provides all UI screens |

**Backend API:** `HC.LIS.API` at `/api/v1/` — Swagger spec at `/swagger/v1/swagger.json`

**Auth:** JWT via HttpOnly cookie `ACCESS_TOKEN` — the SPA never reads the token value directly.

---

## 2. Monorepo & Package Architecture

### 2.1 Repository Structure

```
src/
  HC.LIS/                            # .NET backend (existing)
  HC.LIS.Frontend/                   # Frontend monorepo root (new)
    package.json                     # Yarn workspace root: "workspaces": ["packages/*"]
    yarn.lock
    .yarnrc.yml
    tsconfig.base.json               # Shared TS compiler options (strict: true)
    packages/
      hc-lis-api-client/
        package.json                 # name: "@hc-lis/api-client"
        vite.config.ts               # lib mode — entry: src/index.ts
        openapi-ts.config.ts         # @hey-api/openapi-ts — reads swagger.json, outputs src/generated/
        vitest.config.ts
        src/
          client.ts                  # configureClient() — base URL + credentials: 'include'
          index.ts                   # re-exports all generated clients + DTOs
          generated/                 # codegen output — gitignored, regenerated in CI
      hc-lis-spa/
        package.json                 # "@hc-lis/api-client": "workspace:*"
        angular.json
        proxy.conf.json              # dev proxy: /api → http://localhost:5000
        playwright.config.ts
        src/
          app/
            core/                    # auth service, guards, shell layout
            features/
              auth/                  # login screen
              orders/                # test order request
              waiting-room/          # sample collection
              worklist/              # doctor worklist
              admin/                 # user management (ITAdmin)
            shared/                  # pipes, UI primitives
          environments/
        e2e/                         # Playwright specs
```

### 2.2 `hc-lis-api-client` — SDK Package

- **Generation tool:** `@hey-api/openapi-ts` with `@hey-api/client-fetch`
- **Generation command:** `yarn workspace @hc-lis/api-client generate`
  - Reads `/swagger/v1/swagger.json` (dev) or `swagger.json` snapshot (CI)
  - Outputs typed `fetch`-based functions grouped by API tag (auth, orders, collectionRequests, worklistItems, users)
- **`client.ts`** exports `configureClient(baseUrl: string)` — sets base URL and `credentials: 'include'` on all requests; called once in `app.config.ts`
- **Build:** Vite library mode produces `dist/` with ESM + CJS bundles
- **Gitignore:** `src/generated/` is not committed — regenerated as part of CI and local dev setup

### 2.3 `hc-lis-spa` — Angular SPA Package

- Angular 17+ standalone components — no NgModules
- Imports `@hc-lis/api-client` as a Yarn workspace dependency
- All domain API calls routed through SDK functions — no raw `HttpClient` usage for HC.LIS API calls
- Dev server uses `proxy.conf.json` to proxy `/api` → `http://localhost:5000`

---

## 3. Authentication & Authorization

### 3.1 Login Flow

1. User submits email + password via `LoginComponent`
2. `AuthService.login()` calls SDK `login({ body: { email, password } })` → `POST /api/v1/auth/login`
3. API sets `ACCESS_TOKEN` HttpOnly cookie — token value is never accessible from JavaScript
4. On success, `AuthService` calls `me()` → `GET /api/v1/auth/me` to fetch `{ userId, userName, role }`
5. `currentUser = signal<UserSession | null>(null)` is set; router redirects to role home
6. On page reload, `APP_INITIALIZER` calls `me()` to restore session before routes are activated

### 3.2 Logout Flow

1. `AuthService.logout()` calls `POST /api/v1/auth/logout` (clears HttpOnly cookie server-side)
2. `currentUser` signal cleared; router redirects to `/login`

### 3.3 Route Guards

| Guard | Logic |
|---|---|
| `authGuard` | If `authService.currentUser()` is `null`, redirect to `/login` |
| `roleGuard(role: UserRole)` | If `currentUser()?.role !== role`, redirect to `/unauthorized` |

Guards are functional (`CanActivateFn`) — no class-based guards.

### 3.4 Role-to-Screen Mapping

| Screen | Route | Allowed Roles |
|---|---|---|
| Login | `/login` | Anonymous |
| Test Order Request | `/orders/new` | `Receptionist` |
| Waiting Room | `/waiting-room` | `LabTechnician` |
| Doctor Worklist | `/worklist` | `Physician` |
| User Management | `/admin/users` | `ITAdmin` |
| Unauthorized | `/unauthorized` | Any authenticated |

### 3.5 `UserSession` Interface

```typescript
interface UserSession {
  userId: string;
  userName: string;
  role: 'LabTechnician' | 'ITAdmin' | 'Physician' | 'Receptionist';
}
```

---

## 4. Screens & Features

### 4.1 Login (`features/auth/`)

| Artifact | Description |
|---|---|
| `LoginComponent` | Standalone component — reactive form with email + password fields; shows validation errors and API error messages |
| `AuthService` | `login(email, password)` → SDK `login()`; `me()` → SDK `me()`; `logout()`; `currentUser` signal |

**Post-login redirect:**
- `Receptionist` → `/orders/new`
- `LabTechnician` → `/waiting-room`
- `Physician` → `/worklist`
- `ITAdmin` → `/admin/users`

### 4.2 Test Order Request (`features/orders/`)

| Artifact | Description |
|---|---|
| `NewOrderComponent` | Form to create a new order (patient ID input) + inline exam request form |
| `RequestExamFormComponent` | Sub-form for adding one exam item (exam mnemonic, container type, urgent flag) |
| `OrdersService` | `createOrder(patientId)` → SDK `createOrder()`; `requestExam(orderId, examData)` → SDK `requestExam()` |

**State:** `order = signal<OrderDetailsResponse | null>(null)` — updated after creation.

**Workflow:** Fill patient ID → `createOrder()` → add exam items via `requestExam()` (repeatable) → submit confirmation.

### 4.3 Waiting Room / Sample Collection (`features/waiting-room/`)

| Artifact | Description |
|---|---|
| `WaitingRoomComponent` | Displays paginated queue of waiting patients (collection requests with `status=Waiting`) |
| `PatientCardComponent` | Row component showing patient info + action buttons (Call, View) |
| `CollectSampleFormComponent` | Form for creating barcode + recording sample collection |
| `CollectionRequestsService` | `loadQueue()` → SDK `listCollectionRequests({ query: { status: 'Waiting' } })`; `callPatient(id)`; `createBarcode(id, data)`; `recordCollection(id, data)` |

**State:** `queue = signal<CollectionRequestSummary[]>([])` — refreshed on load and after each action.

**Workflow:** Load queue → call patient → create barcode → record collection → patient leaves queue.

> **API prerequisite:** Requires `GET /api/v1/collection-requests` list endpoint (Phase 0, Task 0.1).

### 4.4 Doctor Worklist (`features/worklist/`)

| Artifact | Description |
|---|---|
| `WorklistComponent` | Table of worklist items with status badges and manual Refresh button |
| `WorklistItemDetailComponent` | Detail panel/modal showing result value, report path; Sign Report action |
| `WorklistService` | `loadItems()` → SDK `listWorklistItems()`; `getItemDetails(id)`; `signReport(id, data)` |

**State:** `items = signal<WorklistItemSummary[]>([])`, `selectedItem = signal<WorklistItemDetailsResponse | null>(null)`.

**Refresh:** Manual — user clicks Refresh button; no background polling in v1. WebSocket real-time updates deferred (see Section 10).

> **API prerequisite:** Requires `GET /api/v1/worklist-items` list endpoint (Phase 0, Task 0.2).

### 4.5 User Management (`features/admin/`)

| Artifact | Description |
|---|---|
| `UserListComponent` | Paginated table of users; inline role change action |
| `CreateUserFormComponent` | Form for creating a new user (email, full name, role) |
| `UsersService` | `listUsers()` → SDK `listUsers()`; `createUser(data)`; `changeRole(userId, role)` |

---

## 5. API Integration Layer (SDK)

### 5.1 Code Generation

| Setting | Value |
|---|---|
| Tool | `@hey-api/openapi-ts` v0.x |
| HTTP client | `@hey-api/client-fetch` |
| Input | `/swagger/v1/swagger.json` (dev) or `./swagger.json` snapshot (CI) |
| Output | `packages/hc-lis-api-client/src/generated/` |
| Output format | TypeScript (ESM) — one file per API tag group |

Generated exports: typed request/response interfaces + `fetch`-based function per endpoint.

### 5.2 Client Configuration

```typescript
// packages/hc-lis-api-client/src/client.ts
import { createClient, createConfig } from '@hey-api/client-fetch';

export function configureClient(baseUrl: string): void {
  createClient(createConfig({
    baseUrl,
    credentials: 'include',   // required for HttpOnly cookie auth
  }));
}
```

Called once in `app.config.ts`:
```typescript
configureClient(environment.apiUrl);
```

### 5.3 Error Handling

SDK response interceptor maps ProblemDetails `{ status, detail }` → `ApiError` — thrown as a typed error for Angular services to catch and expose via error signals.

```typescript
// Registered in configureClient()
client.interceptors.response.use((response) => {
  if (!response.ok) throw new ApiError(response.status, await response.json());
  return response;
});
```

### 5.4 No Raw `HttpClient` for API Calls

All HC.LIS API communication goes through SDK functions. Angular's `HttpClient` is retained only for non-API HTTP calls (e.g., loading `assets/config.json`).

---

## 6. HIPAA Compliance

| Rule | Implementation |
|---|---|
| Token never in JS-accessible storage | JWT lives in HttpOnly cookie only — never `localStorage`, `sessionStorage`, or JS variables |
| No PHI in URL path | Patient IDs, sample IDs, and other identifiers are passed in request bodies or query params — never embedded in route path segments that appear in browser history |
| All routes require auth | `authGuard` on every route except `/login` and `/unauthorized` |
| No PHI in logs | `console.log` / `console.error` calls must not include patient names, DOBs, or identifiers — validated by `hipaa.spec.ts` E2E test |
| HTTPS enforced | Production deployment uses HTTPS; cookies are `Secure` + `SameSite=Strict` (enforced by API) |

---

## 7. Angular Project Configuration

### 7.1 `app.config.ts`

```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideAnimations(),
    {
      provide: APP_INITIALIZER,
      useFactory: (auth: AuthService) => () => auth.me().catch(() => null),
      deps: [AuthService],
      multi: true,
    },
  ],
};

configureClient(environment.apiUrl);
```

### 7.2 Environment Files

| File | `apiUrl` | `production` |
|---|---|---|
| `environment.ts` | `http://localhost:5000` | `false` |
| `environment.prod.ts` | `` (empty — same origin) | `true` |

### 7.3 Proxy Config (`proxy.conf.json`)

```json
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true
  }
}
```

### 7.4 TypeScript

`tsconfig.json` extends `../../tsconfig.base.json` which sets `"strict": true`, `"noImplicitAny": true`, `"strictNullChecks": true`.

---

## 8. Routing

```typescript
// src/app/app.routes.ts
export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'orders/new',
    loadComponent: () =>
      import('./features/orders/new-order.component').then(m => m.NewOrderComponent),
    canActivate: [authGuard, roleGuard('Receptionist')],
  },
  {
    path: 'waiting-room',
    loadComponent: () =>
      import('./features/waiting-room/waiting-room.component').then(m => m.WaitingRoomComponent),
    canActivate: [authGuard, roleGuard('LabTechnician')],
  },
  {
    path: 'worklist',
    loadComponent: () =>
      import('./features/worklist/worklist.component').then(m => m.WorklistComponent),
    canActivate: [authGuard, roleGuard('Physician')],
  },
  {
    path: 'admin/users',
    loadComponent: () =>
      import('./features/admin/user-list.component').then(m => m.UserListComponent),
    canActivate: [authGuard, roleGuard('ITAdmin')],
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./core/unauthorized.component').then(m => m.UnauthorizedComponent),
    canActivate: [authGuard],
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  {
    path: '**',
    loadComponent: () =>
      import('./core/not-found.component').then(m => m.NotFoundComponent),
  },
];
```

All feature routes use lazy `loadComponent` for code-splitting. `pathMatch: 'full'` on the root redirect.

---

## 9. Testing

### 9.1 Unit Tests — Jasmine + Karma (`hc-lis-spa`)

Scope: individual services and guards in full isolation. SDK functions are replaced with Jasmine spies.

| Subject | Test Cases |
|---|---|
| `AuthService` | `login()` sets `currentUser` signal on success; login error propagates without setting signal; `logout()` clears signal; `me()` restores session from `GET /auth/me` |
| `authGuard` | Unauthenticated user → `UrlTree` to `/login`; authenticated user → `true` |
| `roleGuard('Receptionist')` | Wrong role → `UrlTree` to `/unauthorized`; matching role → `true` |
| `OrdersService` | `createOrder()` calls SDK `createOrder()` with correct body; maps `CreatedIdResponse` to `order` signal |
| `CollectionRequestsService` | `loadQueue()` calls SDK list with `status=Waiting` param; `callPatient()` calls SDK action endpoint |
| `WorklistService` | `loadItems()` calls SDK list endpoint; `signReport()` calls SDK sign endpoint |
| SDK `configureClient` — vitest in `hc-lis-api-client` | `configureClient()` sets `baseUrl`; all requests include `credentials: 'include'` |

Run: `yarn workspace hc-lis-spa test`

### 9.2 Integration Tests — Angular TestBed + `HttpClientTestingModule` (`hc-lis-spa`)

Scope: component + service wiring with real Angular DI; HTTP intercepted via `HttpTestingController`. SDK functions proxied through `HttpClientTestingModule` to validate rendering and state.

| Scenario | Assertions |
|---|---|
| `LoginComponent` — valid credentials | Router navigates to role-appropriate home route after `login()` resolves |
| `LoginComponent` — invalid credentials | Error message rendered; `currentUser` signal remains `null` |
| `NewOrderComponent` — form submit | `OrdersService.createOrder()` dispatched with correct patient ID; confirmation element visible in DOM |
| `WaitingRoomComponent` — with patients | Queue list renders one row per `CollectionRequestSummary` item |
| `WaitingRoomComponent` — empty | Empty-state element visible when queue is empty |
| `WorklistComponent` — with items | Table renders rows with `status` badge; clicking row emits detail load |

Run: `yarn workspace hc-lis-spa test` (same `ng test` runner; integration tests co-located with components as `*.integration.spec.ts`)

### 9.3 E2E Tests — Playwright (`hc-lis-spa/e2e/`)

Scope: full golden-path flows against running Angular SPA + real HC.LIS.API backend.

Config: `playwright.config.ts` — `baseURL` from `E2E_BASE_URL` env var (default `http://localhost:4200`); 3 projects (Chromium, Firefox, WebKit); `screenshot: 'only-on-failure'`.

| Spec file | Flow | Role |
|---|---|---|
| `auth.spec.ts` | Login with valid credentials → redirected to role home screen | All |
| `auth.spec.ts` | Login with invalid credentials → error message visible; stays on `/login` | All |
| `orders.spec.ts` | Login as Receptionist → fill order form → add exam → submit → confirmation visible | Receptionist |
| `waiting-room.spec.ts` | Login as LabTechnician → waiting room loads with queue → call patient → create barcode → record collection → patient removed from queue | LabTechnician |
| `worklist.spec.ts` | Login as Physician → worklist loads → click Refresh → rows visible → click row → details panel opens → sign report | Physician |
| `admin-users.spec.ts` | Login as ITAdmin → user list loads → create user form → submit → new user appears in list | ITAdmin |
| `hipaa.spec.ts` | After login, page URL contains no patient names or DOBs; `localStorage` and `sessionStorage` contain no `ACCESS_TOKEN` | Any |

Run: `yarn workspace hc-lis-spa e2e`

---

## 10. Open Design Decisions

| # | Decision | Options | Recommendation |
|---|---|---|---|
| 1 | Real-time worklist updates | WebSocket (SignalR / native WS) vs polling vs manual refresh | Manual refresh for v1; add WebSocket in the next iteration when backend event streaming is ready — Angular Signals integrate cleanly with Observable-based streams |
| 2 | SDK `generated/` directory | Commit to git vs gitignore + regenerate in CI | Gitignore `src/generated/` — regenerate via `yarn workspace @hc-lis/api-client generate` in CI pipeline and `postinstall` script; avoids merge conflicts on API changes |
| 3 | Error display strategy | Toast notifications vs inline error signals vs global error boundary | Inline error signals on each feature service for v1 (simplest); add a toast service in a future pass |
| 4 | UI component library | No library (plain CSS), Angular Material, or PrimeNG | Unresolved — ship unstyled functional components for v1; evaluate Angular Material v17 (`@angular/material`) for v2 |
