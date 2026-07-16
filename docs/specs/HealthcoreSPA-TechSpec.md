# Technical Spec: HealthcoreSPA

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-05-03
**PRD Reference:** [docs/prd/HealthcoreSPA.md](../prd/HealthcoreSPA.md)

---

## 1. Overview

HealthcoreSPA is a browser-only Single Page Application that surfaces every HC.LIS workflow to non-technical lab staff through a role-scoped, HIPAA-compliant UI. It lives in a Yarn workspaces monorepo at `src/HC.LIS.Frontend/` alongside the existing .NET backend.

**Technology:** Angular 21 (standalone components), TypeScript, Yarn workspaces, Vite, Playwright

**Monorepo packages:**

| Package | Path | Purpose |
|---|---|---|
| `hc-lis-api-client` | `packages/hc-lis-api-client/` | TypeScript SDK — auto-generated from HC.LIS.API OpenAPI spec; Vite library mode |
| `hc-lis-spa` | `packages/hc-lis-spa/` | Angular 21 SPA — consumes the SDK; provides all UI screens |

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
        package.json                 # "@hc-lis/api-client": "file:../hc-lis-api-client"
        angular.json
        proxy.conf.json              # dev proxy: /api → http://localhost:5000
        playwright.config.ts
        src/
          app/
            core/
              domain/                # Pure TS interfaces & value objects (no Angular imports)
              application/           # Injectable services + port interfaces (no SDK imports)
              infrastructure/        # SDK adapters implementing port interfaces
              guards/                # authGuard, roleGuard (presentation-layer plumbing)
              shell/                 # ShellComponent, UnauthorizedComponent, NotFoundComponent
            features/
              auth/                  # LoginComponent
              orders/                # NewOrderComponent, RequestExamFormComponent
              waiting-room/          # WaitingRoomComponent, PatientCardComponent
              worklist/              # WorklistComponent, WorklistItemDetailComponent
              admin/                 # UserListComponent, CreateUserFormComponent
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

- Angular 21 standalone components — no NgModules
- Imports `@hc-lis/api-client` as a local file dependency (`file:../hc-lis-api-client`)
- All domain API calls routed through service port interfaces — components never import the SDK directly
- Dev server uses `proxy.conf.json` to proxy `/api` → `http://localhost:5000`
- Unit tests use **vitest** (Angular 21 default — replaces Karma/Jasmine)

### 2.4 Frontend Clean Architecture

The SPA follows Clean Architecture with four concentric layers. Framework dependencies are confined to the outermost layer.

#### Layers

| Layer | Path | Allowed dependencies |
|---|---|---|
| **Domain** | `src/app/core/domain/` | None — pure TypeScript interfaces and value objects |
| **Application** | `src/app/core/application/` | Domain only — injectable services + port interfaces |
| **Infrastructure** | `src/app/core/infrastructure/` | Application + `@hc-lis/api-client` SDK — implements ports |
| **Presentation** | `src/app/features/` + `src/app/core/shell/` + `src/app/core/guards/` | Application layer services + Angular framework |
| **UI primitives** | `src/app/ui/` | Angular framework + design tokens only — no application/domain imports (see §2.5) |

#### Rules

- Domain models (`UserSession`, `OrderSummary`, etc.) live in `core/domain/` with **zero Angular imports**.
- Services (`AuthService`, `OrdersService`, etc.) live in `core/application/`, are `@Injectable`, and depend only on port interfaces — never the SDK directly.
- SDK adapter classes in `core/infrastructure/` implement port interfaces and call SDK functions — this is the **only layer that imports `@hc-lis/api-client`**.
- Angular components in `features/` inject application-layer services; they never import SDK functions or HTTP clients.
- Route guards and `APP_INITIALIZER` live in `core/guards/` and `app.config.ts` — considered presentation-layer plumbing.

#### Example: Auth

```
core/domain/         UserSession.ts            interface { userId, userName, role } — no Angular
core/application/    IAuthPort.ts              port interface
                     AuthService.ts            @Injectable — login(), me(), logout(), currentUser signal
core/infrastructure/ SdkAuthAdapter.ts         implements IAuthPort — calls SDK login/me functions
features/auth/       LoginComponent            injects AuthService; zero SDK imports
```

#### Why

- Components are **unit-testable with simple mock services** — no SDK spies required
- Swapping the SDK or HTTP transport requires changing only `core/infrastructure/`
- Domain interfaces are portable and reusable in non-Angular contexts (SSR, web workers)
- Mirrors the HC.LIS backend's own Clean Architecture/CQRS layering for conceptual consistency

### 2.5 UI Component Library (`src/app/ui/`)

A shadcn-blueprinted, hand-rolled set of standalone Angular primitives — **no Tailwind, no Angular
Material, no third-party component kit**. Each primitive uses the `hc-` selector prefix, signal
inputs, external `.html`/`.css`, styles **exclusively** from design tokens (`var(--color-*)` etc.),
and passes through `data-testid`. Primitives never import application/domain services — they are pure
presentation and are unit-tested with vitest.

| Primitive | Notes |
|---|---|
| `button` | variants: default / cta / ghost / destructive; `loading`, `disabled`, `size="icon"` |
| `input` + `label` + `field` | field wraps label + control + helper/error text; validate-on-blur |
| `select` / `native-select` | token-styled form controls |
| `badge` | role + status variants (shell role badge, order/worklist statuses) |
| `card` | triage cards, detail panels, print-labels card |
| `table` | sticky header, `tabular-nums`, `aria-sort`, dense mode |
| `dialog` | focus trap, Esc-to-close, 40–60% scrim (print-labels modal, reason capture, confirms) |
| `sheet` | right-anchored slide-over on native `<dialog>` (patient detail from search) |
| `combobox` | patient-picker typeahead |
| `tabs` | patient-/order-detail sections |
| `alert` + `toast` | sonner-style toaster, `aria-live="polite"`, 3–5s auto-dismiss |
| `skeleton`, `spinner` | loading states (>300ms rule) |
| `pagination` | 1-based `page`/`pageCount`, windowed page list, `nav[aria-label]` landmark |
| `dropdown-menu` | root + `[hc-dropdown-trigger]` + `[hc-dropdown-item]`; roving menu a11y (row actions, user menu) |
| `avatar` | initials fallback + optional image; shell user menu |
| `breadcrumb` | `items`-driven trail; last crumb is `aria-current="page"` (`/orders/:id`, `/patients/:id`) |
| `command` | Ctrl/Cmd-K palette; combobox + `aria-activedescendant` listbox; renders what it is given (consumer filters) |
| `tooltip`, `separator`, `empty` | icon-only buttons, dividers, empty lists |
| `icon` | consolidated inline SVGs, Lucide-style, 1.5px stroke |
| `motion/` | GSAP helpers — see §10.4 |

> **Content projection gotcha:** a primitive that projects consumer-authored elements
> (`dropdown-menu`, `card`, `table`) cannot style them with plain scoped selectors — projected
> nodes carry the *consumer's* `_ngcontent` attribute. Style those parts via `:host ::ng-deep`.

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

### 9.1 Unit Tests — Vitest via `@angular/build:unit-test` (`hc-lis-spa`)

Scope: individual services, guards, and every `ui/` primitive in full isolation. SDK functions are
replaced with test doubles. All `ui/` primitives ship a co-located `*.spec.ts` (anatomy, variants,
states, ARIA). Run once (no watch): `yarn workspace hc-lis-spa test --no-watch`.

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
| `order-patient-picker.spec.ts` | New-order patient-picker combobox typeahead selects a patient | Receptionist |
| `patients.spec.ts` | Patient search → register → detail | Receptionist |
| `triage.spec.ts` | Login as LabTechnician → triage loads with queue → call patient → create barcode → record collection → patient removed from queue | LabTechnician |
| `worklist.spec.ts` | Login as Physician → worklist loads → click Refresh → rows visible → click row → details panel opens → sign report | Physician |
| `admin-users.spec.ts` | Login as ITAdmin → user list loads → create user form → submit → new user appears in list | ITAdmin |
| `nav.spec.ts` | Sidebar nav active-state indicator per role | Role-scoped |
| `hipaa.spec.ts` | After login, page URL contains no patient names or DOBs; `localStorage`/`sessionStorage` contain no `ACCESS_TOKEN` | Any |

**Design System v2 gates** (Phase 0 + Phase 4):

| Spec file | Gate |
|---|---|
| `theme.spec.ts` | Toggle switches `data-theme` on `<html>`, persists across reload, label/icon swaps, works per role |
| `a11y.spec.ts` | `@axe-core/playwright` scan of every static route with **no WCAG 2 A/AA violations**, in light **and** dark, at desktop (1280) **and** 375px (40 scans) |
| `reduced-motion.spec.ts` | With `emulateMedia({ reducedMotion: 'reduce' })`, every GSAP entrance/crossfade/stagger is skipped and content settles at full opacity |

Run: `yarn workspace hc-lis-spa e2e`

**Known infra debt — full-suite cross-browser flakiness.** Running all three browsers
back-to-back at `workers: 1` (306 tests), webkit (which runs last) intermittently starves —
`getByLabel('Email')` / `waitForResponse` time out on ~12 `auth`/`hipaa` tests, while webkit's
own `a11y`/`theme`/`reduced-motion` specs (which also log in) pass in the same run. The same 12
specs pass 48/48 when run in isolation, so it is transient contention (dev-server recompilation
and/or API request-latency spikes under sustained load), not a product defect. This is the milder
residue of the historical `SqlConnectionFactory` pool-exhaustion hang, now that the leak is fixed
(Postgres held steady at ~38 connections across the full run). Local runs use `retries: 0`; CI
uses `retries: 2`, which absorbs it. Future hardening: enable a local retry, or run e2e against a
production `ng build` served statically instead of `ng serve`.

---

## 10. Design System v2, Theming & Motion

Archetype **"Accessible & Ethical"** — WCAG AA minimum, AAA where cheap; light and dark tuned
independently. Anti-patterns: no neon, no purple/pink gradients, no motion-heavy choreography, no
emoji icons.

### 10.1 Identity

- **Colors:** primary cyan `#0891B2` (accent), health-green `#059669` (CTA/confirm). Dark theme
  lifts these to `#22D3EE` / `#34D399` for contrast on slate surfaces.
- **Type:** Figtree (headings) + Noto Sans (body), JetBrains Mono for barcodes/mono data. A
  `tabular-nums` utility (`font-variant-numeric: tabular-nums`) is used for data tables/timers.

### 10.2 Token strategy — single seam

All theming lives in **`src/styles.css`**: the `:root` block (light) and the `[data-theme="dark"]`
block (dark, desaturated/lightened variants — never inverted). Token **names are stable**; the v2
refactor changed **values** only and added new tokens (`--color-cta`, `--color-cta-bg`, `--motion-*`)
plus dark overrides for semantic (`--color-error/warning/success` + `-bg`) and `--color-role-*`
tokens. Components consume `var(--color-*)` exclusively — no raw hex in component CSS. Contrast for
every fg/bg pair in both themes is computed, not eyeballed, and gated by `a11y.spec.ts` (§9.3).

### 10.3 Theming mechanism

| Seam | Owner |
|---|---|
| Storage + `data-theme` on `<html>` + `localStorage['hc-lis-theme']` | `core/application/theme.service.ts` |
| Token overrides | `[data-theme="dark"]` block in `src/styles.css` |
| Toggle UI (`theme-toggle-btn`, sidebar footer) | `core/shell/shell.component.*` |

`ThemeService.init()` runs in the `App` constructor to prevent a flash-of-wrong-theme.

### 10.4 Motion (`ui/motion/motion.ts`)

GSAP, kept deliberately subtle. Tokens `--motion-fast: 150ms` / `--motion-normal: 200ms` /
`--motion-slow: 300ms` (mirrored as `MOTION` in `motion.ts`). Rules, enforced across all call sites
and by `reduced-motion.spec.ts`:

- **transform/opacity only** (`autoAlpha`, `x`, `y`, `scale`) — never `width/height/top/left`.
- micro-interactions 150–300ms; **exits ~60–70% of enter** (e.g. route crossfade 200ms in / 120ms out).
- staggers 30–50ms/item; eases `power1/2.out`.
- **everything behind `prefers-reduced-motion`** — via `gsap.matchMedia` in the `withMotion`/`useMotion`
  Angular helpers (created after first render, `revert()` on destroy), a synchronous
  `prefersReducedMotion()` guard for result-set-keyed staggers, and a global CSS
  `@media (prefers-reduced-motion: reduce)` guard in `styles.css`.

### 10.5 Viewport support — desktop-first

HC.LIS is a clinical-workstation application; the shell is a **fixed desktop sidebar layout with no
responsive breakpoints** (only `prefers-reduced-motion` and `print` media queries exist). Login and
error pages center gracefully down to 375px, but authed shell routes are **not** designed for phone
widths. The `a11y.spec.ts` 375px pass is retained as a contrast/label gate — it does not assert
mobile layout. A responsive/off-canvas navigation is deferred (see §11).

---

## 11. Open Design Decisions

| # | Decision | Options | Recommendation |
|---|---|---|---|
| 1 | Real-time worklist updates | WebSocket (SignalR / native WS) vs polling vs manual refresh | Manual refresh for v1; add WebSocket in the next iteration when backend event streaming is ready — Angular Signals integrate cleanly with Observable-based streams |
| 2 | SDK `generated/` directory | Commit to git vs gitignore + regenerate in CI | Gitignore `src/generated/` — regenerate via `yarn workspace @hc-lis/api-client generate` in CI pipeline and `postinstall` script; avoids merge conflicts on API changes |
| 3 | Error display strategy | Toast notifications vs inline error signals vs global error boundary | **Resolved** — sonner-style `ui/toast` toaster (`aria-live="polite"`) plus inline error signals on feature services |
| 4 | UI component library | No library (plain CSS), Angular Material, or PrimeNG | **Resolved** — hand-rolled shadcn-blueprinted `src/app/ui/` primitives, tokens only (§2.5); no Tailwind/Material |
| 5 | Responsive / mobile shell | Off-canvas hamburger nav vs desktop-only | Desktop-only for now (§10.5); revisit if a mobile/tablet clinical use-case emerges |
