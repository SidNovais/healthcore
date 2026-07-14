# Frontend shadcn-UX Enhancement — Handoff & Progress Tracker

_Last updated: 2026-07-13_

Follow-on to `docs/specs/FrontendRefactor-Handoff.md` (Design System v2, Phases 0–4, already merged).

## Context & direction

**Why:** The question "why didn't the refactor plan mention shadcn?" — it did. The entire `src/app/ui/` primitive library was **blueprinted from the shadcn MCP registry** and hand-rolled as native Angular standalone components. shadcn/ui itself is React-only (Radix + Tailwind + CVA); this app is **Angular 21.2** with pure CSS-variable tokens and no Tailwind, so shadcn can never be a runtime dependency here.

**Direction (user-chosen):**
- **Extend the components we own** (own-the-code → easy to change later). Keep pure Angular + CSS tokens. **No Tailwind, no spartan/ng, no new runtime deps.**
- Use the **shadcn MCP** as the ongoing design/anatomy/a11y blueprint.
- **Focus: page/flow UX upgrades**, delivered as **many small, independently-shippable phases**.

Full plan: `C:\Users\sidne\.claude\plans\ok-now-we-already-prancy-bear.md`.

## Key finding driving the work

The Phase-0–4 refactor left **6 primitives built but never wired up**, and pages were hand-rolling those exact patterns. Only **2 primitives are genuinely missing**.

| Category | Components | Action |
|---|---|---|
| Built but unused (at start) | `combobox`, `dialog`, `tabs`, `toast`, `skeleton`, `separator` | Adopt in early phases (cheap) |
| Missing — must build | `pagination`, `dropdown-menu` | Track 2; blocks the 4 data tables |
| Net-new — build later | `sheet`, `avatar`, `breadcrumb`, `command`, `date-picker` | Tracks 3–5 |

## Universal guardrails (every phase)

- **TDD:** failing Playwright/Vitest test first; `test:` commit precedes `feat:` commit.
- **Never rename an existing `data-testid`** (234 refs; densest: orders 96, triage 32, order-patient-picker 29, patients 27). New wrappers keep the current testids on the same elements. When a primitive can't carry a testid, **extend the primitive** with optional testid inputs (see `hc-combobox`, `ToastService`).
- **a11y both themes:** axe clean in light AND dark for every touched slice.
- **Motion:** GSAP only, transform/opacity, 150–300ms, behind `prefers-reduced-motion` (`ui/motion/motion.ts`).
- **CSS budget:** 4kB warn / 8kB error per component; tokens only, no new hex without an axe-verified contrast check.
- **Boundaries:** components never import `@hc-lis/api-client` directly.

## Verification

Run from `src/HC.LIS.Frontend/packages/hc-lis-spa`:
- `yarn build` — type-checks templates + enforces CSS budgets. **Baseline: clean** (one pre-existing jsbarcode CommonJS warning, unrelated).
- `yarn test --watch=false` — Vitest. **Baseline on main: 132 tests.**
- `yarn e2e` — Playwright; **needs `ng serve` + API + DB up** (not run during phase dev; each phase notes which spec is its gate).

## shadcn MCP workflow per phase

1. `mcp__shadcn__view_items_in_registries` + `get_item_examples_from_registries` → pull current anatomy/a11y/keyboard contract (translate to Angular + tokens, don't copy React/Tailwind).
2. Implement/extend the Angular primitive; wire into the page.
3. `mcp__shadcn__get_audit_checklist` before the a11y/e2e gate.

## Branching model

**Each phase branches off `main` independently** (one PR per phase). Phases do **not** stack — the working tree resets to `main` on each checkout. Merge order is the user's call.

---

## Phase status

Legend: ✅ done · 🔀 merged to `main` · 🔜 next · ⬜ planned

### Track 1 — adopt already-built primitives 🔀 **merged to `main`** (merge commit `ee54cde`)
- ✅ **Phase 1 — Triage** · branch `feat/frontend-phase-1-triage-adopt-primitives`
  Filter bar → `hc-tabs`; print-labels-modal → `hc-dialog` + `hc-skeleton`; empty divs → `hc-empty`. Build + 132 tests green.
- ✅ **Phase 2 — New-order** · branch `feat/frontend-phase-2-order-combobox-toast`
  patient-picker → `hc-combobox` (extended it with `inputTestId`/`listboxTestId`/`optionTestId` to preserve the picker's testids); exam confirmation → `hc-toast`. **`ToastService.show` gained an optional `testId` + dedupe; `hc-toaster` is now mounted at the app root — it was never mounted before, so no toast could render.** Build + 135 tests green.
- ✅ **Phase 3 — List-state consistency** · branch `feat/frontend-phase-3-list-states`
  order-list empty + order-detail empty-items → `hc-empty` (kept `empty-items` testid); order-detail Accept/Reject/Cancel/On-Hold → success toasts (error alert unchanged). Build + 133 tests green.
- ✅ **Phase 3b — Loading skeletons** · branch `feat/frontend-phase-3b-loading-skeletons`
  Added a loading signal per list service — `OrdersService.loadingList`, `WorklistService.loading`, `UsersService.loading`, `PatientsService.searching` (search only; detail load untouched) — each wrapping its fetch in `try/finally` so the signal always resets, even on reject. Wired `hc-skeleton` rows into all 4 list tables (order-list, worklist, patient-search, user-list) via a 3-branch template `@if(loading){skeleton} @else if(empty){empty} @else{rows}`. Skeleton rows carry **new** `*-skeleton-row` testids (no existing testid renamed); the live region is `aria-busy` while loading, `hc-skeleton` stays `aria-hidden`. New integration specs for order-list + user-list (neither had one); worklist + patient-search specs gained loading-signal mocks. Build clean (CSS budgets ok), **154 Vitest tests green** (132→154, +22). e2e not run (needs stack); gates: `orders` / `worklist` / `patients` / `admin-users` specs — all `waitForResponse`, so the transient skeleton is low-risk. Note: `listUsers` toggles `loading`, so create-user/role-change refreshes briefly flash the skeleton (acceptable — the table is genuinely reloading).

### Track 2 — build the 2 missing primitives + upgrade the 4 tables 🔀 **merged to `main`** (merge commit `d8f88f5`)
- ✅ **Phase 4 — build `hc-pagination`** (`ui/pagination/`). 1-based `page`/`pageCount` inputs, `pageChange` output, `testId` prefix for child testids (`{testId}-prev`/`-next`/`-page-{n}`/`-ellipsis`). `nav[aria-label]` landmark, active page `aria-current="page"`, prev/next disabled at bounds, ranges >7 collapse to `1 … c-1 c c+1 … n` with `aria-hidden` ellipsis, tabular-nums. Extended `hc-icon` with `chevron-left`/`chevron-right`. **6 Vitest specs; suite 164 green** (158→164); build clean.
- ✅ **Phase 5 — build `hc-dropdown-menu`** (`ui/dropdown-menu/`). Three parts: root `hc-dropdown-menu` (owns open state, click-outside via `document:click` + host `contains`, `document:keydown.escape`, focus return), `button[hc-dropdown-trigger]` (`aria-haspopup=menu`, `aria-expanded`, ArrowDown opens+focuses first), `button[hc-dropdown-item]` (`role=menuitem`, tabindex −1, click/Enter/Space activates → emits `select` + closes, Arrow up/down wrap focus). `testId` prefixes the menu container testid; GSAP fade-in behind `prefers-reduced-motion`. Hand-rolled (no spartan). **6 Vitest specs; suite 170 green** (164→170); build clean.
- ✅ **Phase 6 — Orders table**. Client-side column sorting (`patientName`/`requestedBy`/`orderPriority`/`requestedAt`/`itemCount`; header buttons `order-list-sort-*`, `th[aria-sort]` reflects state, toggles asc↔desc, resets to page 1); client-side pagination via `hc-pagination` (`PAGE_SIZE=10`, `displayPage` clamps a shrinking set, control only rendered when `pageCount > 1`, `order-list-pagination` testid); per-row `hc-dropdown-menu` action (`order-actions-trigger` → `order-action-view` navigates to detail) inside a `stopPropagation` actions cell so the row's routerLink still navigates on row-click. Added `more-horizontal` icon. **No existing testid renamed** (`order-list-table`/`order-list-row`/`patient-name-cell` intact). 4 new integration specs + 1 e2e (row-action View nav). Suite **174 green** (170→174); build clean (order-list CSS 4.4 kB, under the 8 kB error budget). e2e gate `orders.spec.ts` — run before merge.
- ✅ **Phase 7 — Patients table**. Client-side pagination via `hc-pagination` (`PAGE_SIZE=10`, `patient-search-pagination`, only when `pageCount > 1`, resets to page 1 on new input/clear); per-row `hc-dropdown-menu` (`patient-actions-trigger` → `patient-action-view`) in a `stopPropagation` cell so the row's `(click)` navigation survives; search-bar polish — leading `search` icon + a `patient-search-clear` (`x`) button shown once a term is typed that clears the field and re-runs `search('')`. Skeleton already present (Phase 3b). **No existing testid renamed** (`patient-search-input`/`patient-row`/`patient-search-empty-state`/`patient-search-results`/`patient-search-skeleton-row` intact). 4 new integration specs + 1 e2e (row-action View nav). Suite **178 green** (174→178); build clean (patient-search CSS 2.5 kB). Gate `patients.spec.ts`.
- ✅ **Phase 8 — Worklist table**. Client-side column sorting (`sampleBarcode`/`examCode`/`patientName`/`status`/`createdAt`; header buttons `worklist-sort-*`, `th[aria-sort]`, asc↔desc toggle) + pagination via `hc-pagination` (`PAGE_SIZE=10`, `worklist-pagination`, only when `pageCount > 1`) + per-row `hc-dropdown-menu` (`worklist-actions-trigger` → `worklist-action-view` selects the item into the inline detail panel) in a `stopPropagation` cell so the row's own select-on-click still works. **Inline detail panel kept (desktop-only)**; no existing testid renamed (`worklist-row`/`worklist-title`/`refresh-btn`/`patient-name-cell`/`empty-state`/`worklist-skeleton-row` intact). 4 new integration specs + 1 `test.fixme` e2e (seed-data-gated like the sign-report flow). Suite **182 green** (178→182); build clean (worklist CSS 3.8 kB). Gate `worklist.spec.ts`.
- ✅ **Phase 9 — Admin/users table**. Raw per-row role `<select>` → `hc-dropdown-menu` (`user-role-trigger` shows the current role; `user-role-option-{Role}` items) → picking a different role opens an `hc-dialog` confirm (`role-change-dialog`, `confirm-role-change-btn`/`cancel-role-change-btn`) naming the from→to change; confirm calls `changeRole` **and fires the deferred success toast** (`role-change-toast`, dedupe testId). Create-user form moved into an `hc-dialog` (`create-user-dialog`, form lazy-`@if`'d so `create-user-btn` → `create-user-form` visible flow is preserved). Client-side pagination via `hc-pagination` (`users-pagination`, `PAGE_SIZE=10`, only when `pageCount > 1`). **No existing testid renamed** (`create-user-btn`/`create-user-form`/`submit-create-user-btn`/`user-row`/`users-title`/`users-skeleton-row`/`empty-state` intact; the old role `<select>` carried no testid). 5 new integration specs + 1 e2e (create → change-role → confirm → toast, self-contained so no seed-user role is mutated). Suite **187 green** (182→187); build clean (user-list CSS 3.6 kB). Gate `admin-users.spec.ts`.

**Track 2 complete & merged to `main`** (merge `d8f88f5`). 6 phases, one commit-pair each. Net: +2 primitives (`hc-pagination`, `hc-dropdown-menu`), +3 icons (`chevron-left/right`, `more-horizontal`), all 4 data tables upgraded. Vitest **132 → 187** (+55); post-merge on `main`: 187 green, build clean. Full `yarn e2e` still to be run against a live stack (per-page gates listed above) — not yet exercised.

### Track 3 — detail slide-overs + order-detail actions
- ✅ **Phase 10 — build `hc-sheet`** (`ui/sheet/`). Right-anchored slide-over on the native `<dialog>` (reuses the browser focus-trap/Esc/top-layer like `hc-dialog`); `open` two-way model, optional `ariaLabel` on the panel, `inset:0 0 0 auto` + `margin-left:auto` pins it to the inline-end edge full-height, GSAP `xPercent:100` slide-in behind `prefers-reduced-motion`. 5 Vitest specs; suite **192 green** (187→192); build clean (sheet CSS well under budget).
- ✅ **Phase 11 — Patient detail slide-over**. Row-click / row-action **View** on the search page now open the patient detail in an `hc-sheet` (URL stays `/patients` — no patientId in the address bar, a HIPAA win) instead of navigating; `PatientDetailComponent` gained an optional `patientId` **input** (input-driven `effect` reload when in sheet mode, `ngOnInit` still drives the routed `/patients/:id` page — kept for the register→detail deep-link flow). Anonymize confirm moved from an inline button into an `hc-dialog` (`anonymize-dialog`, content `@if`-gated per the jsdom gotcha; `patient-anonymize-btn`/`anonymize-confirm-btn` testids preserved, added `cancel-anonymize-btn`). **No existing testid renamed** (`patient-row`/`patient-action-view`/`patient-status-badge`/`patient-edit-btn` intact); new `patient-detail-sheet`. e2e updated to the sheet flow: `patients.spec.ts` row-action + anonymize workflows assert the sheet (URL stays `/patients`), register→edit flow unchanged; `hipaa.spec.ts` detail-load asserts the sheet. Suite **196 green** (192→196); build clean. Gate `patients.spec.ts`, `hipaa.spec.ts` (run against live stack before merge).
- ✅ **Phase 12 — Order detail actions**. Per-exam-item action buttons (Accept/Reject/On Hold/Cancel) collapsed into an `hc-dropdown-menu` (`exam-actions-trigger` → `more-horizontal` icon; items keep their `accept-btn`/`reject-btn`/`on-hold-btn`/`cancel-btn` testids, wired via `(select)`; shown only when the item is non-terminal via a `hasActions(status)` helper — a typed-`string` param dodges the template `||`/`!==` narrowing that TS2367'd). Reject + On Hold reason capture moved from inline row forms into component-level `hc-dialog`s (`reject-dialog`/`on-hold-dialog`, `[open]` bound to `activeRejectItemId()/activeOnHoldItemId() !== null` + `(openChange)` reset on Esc/backdrop/Cancel; content `@if`-gated per the jsdom gotcha). **No existing testid renamed** (`reject-reason-form`/`reject-reason-input`/`confirm-reject-btn`/`cancel-reject-btn` + on-hold equivalents, `item-status`, `exam-action-error`, `exam-item-row` intact). e2e updated: `orders.spec.ts` Accept/Reject/Cancel/OnHold/409 flows now open the row menu first, reason flows assert the dialog. Suite **198 green** (196→198); build clean (order-detail CSS within budget). Gate `orders.spec.ts` (run against live stack before merge).

### Track 4 — shell
- ⬜ **Phase 13 — build `hc-avatar` + shell user menu** (consolidate theme/logout/role into an avatar dropdown). Gate `nav.spec.ts`, `theme.spec.ts`.
- ⬜ **Phase 14 — build `hc-breadcrumb`** + wire `/orders/:id`, `/patients/:id`. Gate `nav.spec.ts`.
- ⬜ **Phase 15 — command palette** (`ui/command/`, Ctrl/Cmd-K, jump-to-patient/order). New `e2e/command-palette.spec.ts`.

### Track 5 — forms
- ⬜ **Phase 16 — Register/patient form**: section grouping with `hc-separator`; build `hc-date-picker` for DOB; consistent `hc-select`. Gate `patients.spec.ts`.

---

## Primitives created/extended so far

- **Extended** `hc-combobox`: optional `inputTestId` / `listboxTestId` / `optionTestId` inputs.
- **Extended** `ToastService.show(msg, { testId })`: optional `testId` on `Toast`, dedupe by testId; `hc-toaster` renders `toast.testId ?? 'toast'`.
- **Mounted** `hc-toaster` at the app root (`app.ts` / `app.html`).
- **Adopted** `hc-skeleton` in the 4 list tables (Phase 3b); added a loading signal per list service (`loadingList` / `loading` / `searching`).
- **Built** `hc-pagination` (Phase 4): `page`/`pageCount`/`testId`/`ariaLabel` inputs, `pageChange` output, ellipsis-truncated windowed page list.
- **Extended** `hc-icon` with `chevron-left` / `chevron-right`.
- **Built** `hc-dropdown-menu` (Phase 5): `hc-dropdown-menu` root + `[hc-dropdown-trigger]` + `[hc-dropdown-item]` directives; `select` output per item; full menu-pattern a11y + GSAP fade behind reduced-motion.
- **Extended** `hc-icon` with `more-horizontal` (row-action affordance, Phase 6).
- **Adopted** `hc-pagination` + `hc-dropdown-menu` in the orders table (Phase 6); added client-side sort/paginate signals to `OrderListComponent`.
- **Adopted** `hc-pagination` + `hc-dropdown-menu` in the patients table (Phase 7); added a clear-button + search-icon affordance to the search field.
- **Adopted** `hc-pagination` + `hc-dropdown-menu` in the worklist table (Phase 8); added client-side sort/paginate signals; inline detail panel preserved.
- **Adopted** `hc-pagination` + `hc-dropdown-menu` + `hc-dialog` in the admin/users table (Phase 9); role change routed through a confirm dialog + success toast; create-user form moved into a dialog.

## Open follow-ups / notes

- Full **e2e has not been run** during phase development (needs the API+DB+ng serve stack). Each phase is gated by build + the full Vitest suite + reasoning about the existing spec assertions; run `yarn e2e` per branch before merging.
- **Track 1 (Phases 1–3b) merged to `main`** on 2026-07-13 via `--no-ff` per-phase merges (commits `b810a64` P1, `45f6842` P2, `20aef05` P3, `ee54cde` P3b). The Phase 3 ↔ 3b overlap on `order-list.component.*` was resolved to the union (skeleton branch + `hc-empty` empty branch). Post-merge baseline on `main`: **158 Vitest tests green**, build clean.
- This tracker now lives on `main`; Track 2+ branches inherit it. **Track 2 develops on a single `feat/frontend-track-2` branch off `main`** (its phases stack: the 4 tables consume the Phase 4/5 primitives), one commit-pair per phase.
- Memory: `project_frontend_shadcn_ux.md` mirrors this status for cross-session continuity.
