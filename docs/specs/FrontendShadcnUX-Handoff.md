# Frontend shadcn-UX Enhancement — Handoff & Progress Tracker

_Last updated: 2026-07-16_

## ⚠️ The e2e gap is closed — and it found five real defects

**`yarn e2e` ran against a live stack for the first time on 2026-07-16** (Docker + migrator +
API + `ng serve`). First result on chromium: **102 passed, 7 failed, 5 skipped**. The 7 failures
were **six distinct root causes** — every one written blind during Tracks 1–5 and never executed.

**Final result: `110 passed, 0 failed, 4 skipped` (chromium, 4.4m). The baseline is green.**
Vitest **264 → 271**, build clean.

| # | Symptom | Root cause | Verdict |
|---|---|---|---|
| **A** | `patient-detail-sheet` "hidden" (3 tests: patients ×2, hipaa) | `hc-sheet`/`hc-dialog` put the testid on the **host**, which is `display:inline` wrapping a `position:fixed` child → box is **0×0**. Measured live: the panel itself renders 448×557. | **Primitive contract bug.** Feature worked; it was untestable. `hc-command` already bound `testId` to its `<dialog>` — sheet/dialog now match. |
| **B** | `role-change-toast` never appears | `PUT /role` returns **409 "Cannot change the role of a user who has not yet activated their account."** The test created a fresh user and immediately reassigned it. | **Invalid test + real product bug.** `confirmRoleChange` awaited with no `catch`, so the rejection escaped and **the user was told nothing at all**. Now surfaces `role-change-error-toast`. |
| **C** | Palette ArrowDown doesn't move the selection | **`gsap.from(dialog, { autoAlpha: 0 })`.** `autoAlpha:0` also sets `visibility:hidden`, and `gsap.from()` applies its start state **immediately** — so it blurred the input focused a line above. Browser trace: `29ms focusin → input`, `32ms focusout ← input` + `visibility:hidden`, `42ms visibility restored, focus not`. ArrowDown then went to `<body>`. | **Real a11y bug — the palette was dead to the keyboard.** Flaky (3/5 then 2/5 failing) only because a keypress landing before ~32ms beat the blur; always passed under reduced-motion, where the tween is skipped. Now fades with `opacity` → **8/8**. |
| **D** | Avatar expected `IT`, got `RO` | `loginAsITAdmin` signs in as **`root@hclis.local`** (the only migration-guaranteed account), not `itadmin@`. | **Test bug.** The avatar was correct. |
| **E** | `getByText(name)` strict-mode violation | Phase 14's breadcrumb trails `Patients / {name}`, so the name matches twice. | **Test bug.** Assertions scoped to the heading. |
| **F** | change-role e2e times out behind its siblings | The heaviest test in the suite (~22s) against Playwright's **30s default**, on a cold API. The file runs in 11s once warm; it always passed in isolation. | **Test budget, not a product failure.** `test.setTimeout(60_000)` on that one test. |

**Root cause C spread further than the one failing test.** `hc-dropdown-menu` (ArrowDown-to-open
focuses `items()[0]`) and `hc-date-picker` (focuses the calendar grid that owns
`aria-activedescendant`) had the **identical focus-then-`autoAlpha`** ordering — both keyboard
paths, neither with e2e coverage, so nothing was failing to reveal them. All three now fade with
`opacity`. **Rule: an entrance tween on anything that takes focus must use `opacity`, never
`autoAlpha`.** The remaining `autoAlpha` tweens (row staggers, route crossfade, login/error
cards) focus nothing and are fine.

**A wrong turn worth recording:** `hc-command`'s reset was first blamed on array identity
(`effect(() => { items(); activeIndex.set(0); })` firing when the shell mints a fresh
`paletteCommands`). That fix — reset keyed on item **content** via `itemsKey` — is retained
because it is correct on its own merits and is pinned by a test, but **it was not the cause**;
the spec stayed flaky after it. The cause was only found by tracing focus events in a real
browser. Instrument before theorising.

**Lesson, now evidenced three times:** Vitest asserts roles/structure (`.not.toBeNull()`), axe
passes unstyled and zero-size elements, and none of it can see a 0×0 box, a blur, or a race.
Only a live browser can. The Track-4 `::ng-deep` bug, root cause **A**, and root cause **C** are
all the same failure mode.

Post-fix: **Vitest 264 → 268**, build clean, chromium e2e re-run as the Phase 0 gate for the
`hc-page` layout work (plan: `C:\Users\sidne\.claude\plans\ok-let-s-create-a-lively-sloth.md`).

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
- `yarn test --watch=false` — Vitest. **Baseline on main after Track 5: 264 tests** (was 132 pre-Track-1).
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

**Track 3 complete** (committed straight to `main`, one `test:`+`feat:` pair per phase). Net: +1 primitive (`hc-sheet`); patient detail now opens as a slide-over from search (no patientId in URL); anonymize + order-item reject/on-hold reasons captured in dialogs; order-item actions in a dropdown. Vitest **187 → 198** (+11). Build clean each phase. Full `yarn e2e` still to be run against a live stack (gates: `patients`/`hipaa`/`orders`).

### Track 4 — shell
- ✅ **Phase 13 — build `hc-avatar` + shell user menu**. `hc-avatar` (`ui/avatar/`) blueprinted from the shadcn avatar anatomy: `name` → initials (first+last for multi-word; **first two chars for a single token**, since `userName` is an email → `itadmin@hclis.local` = "IT"), optional `src` renders an `<img alt=name>` and falls back to initials on error (failure tracked *by src value*, so a new src retries without an effect), `size` → `--hc-avatar-size` custom property the fallback's font-size tracks, fallback `aria-hidden` (decorative — the trigger is named). Shell footer's three loose controls (theme toggle, sign out, role badge) collapsed into one `hc-dropdown-menu` triggered by avatar + user name + caret; identity block is the shadcn `DropdownMenuLabel` slot marked `role="presentation"` so it stays out of the menu's owned-children contract; menu **opens upward** (`bottom: 100%`) since the footer is the last thing in the sidebar. `HcButton`/`HcTooltip` dropped from the shell's imports. **No testid renamed** — `theme-toggle-btn`/`shell-role-badge` keep their ids and just live inside the menu; new `user-menu-trigger`/`user-menu-avatar`/`user-menu-name`/`logout-btn` (sign-out never had one). e2e updated: `theme.spec.ts` opens the menu before each toggle (activating an item closes the menu, so each toggle re-opens); `nav.spec.ts` asserts the badge inside the menu + a new trigger/avatar test. Suite **198 → 211**. Gates `nav.spec.ts`, `theme.spec.ts`.
- ✅ **Phase 14 — build `hc-breadcrumb`** (`ui/breadcrumb/`). Blueprinted from the shadcn breadcrumb contract: `nav[aria-label="Breadcrumb"]` landmark **on the host** (as `hc-pagination` does) wrapping an `<ol>`; last crumb is `aria-current="page"` and **never linked even when it carries a route**, so callers pass a uniform trail without special-casing the tail; separators are decorative `<li aria-hidden>`. **Data-driven `items` input rather than composed sub-components** — these trails are short/static, so consumers stay a single element. Wired: order-detail (replaces the ad-hoc `← Back to Orders`; `RouterLink` dropped from its imports; header now stacks breadcrumb-above-title) and patient-detail (`Patients / {name}` **routed page only** — in slide-over mode `breadcrumbs()` is null because the sheet sits on top of `/patients`, so there's no hierarchy to trail). testids `order-breadcrumb`/`patient-breadcrumb` + `{testId}-link-{i}`/`{testId}-page`. Suite **211 → 220**. Gate `nav.spec.ts`.
- ✅ **Phase 15 — command palette** (`ui/command/`). Blueprinted from the shadcn command anatomy (dialog + input + grouped list), built on the **native `<dialog>` like `hc-sheet`** (browser gives focus trap / Esc / top-layer). a11y = combobox-with-listbox-popup: focus stays in the input, `aria-activedescendant` tracks the active option (so the highlight is **painted explicitly**, not via `:focus`); arrows wrap, Enter selects, changed result set re-activates the top match; GSAP fade behind reduced-motion; content `@if`-gated on `open()` per the jsdom gotcha. **The primitive deliberately does not filter** — consumers mix sync + async matches and a server lookup can match on fields the component never sees (`documentId`), so label-filtering here would silently drop results. Shell: Ctrl-K **and** Cmd-K (either OS may drive), role-aware nav destinations filtered client-side + debounced (200 ms, min 2 chars) patient matches; **patient lookup gated on the role's nav including `/patients`** — the route guard would bounce anyone else, so offering records would leak PHI. Selection routes via each command's own `target` (no id parsing). Added **`PatientsService.quickSearch()`** returning matches directly instead of publishing to `searchResults`/`searching` — those belong to the `/patients` page, which may be rendering *behind* the palette. Also swapped the shell's route-crossfade from raw `window.matchMedia` to the guarded `prefersReducedMotion()` (identical in a browser; the raw call threw in the test DOM). New `e2e/command-palette.spec.ts`. Suite **220 → 243**.

**Track 4 complete** (committed straight to `main`, one `test:`+`feat:` pair per phase). Net: +3 primitives (`hc-avatar`, `hc-breadcrumb`, `hc-command`), +1 service method (`quickSearch`). Vitest **198 → 243** (+45). Build clean each phase. Full `yarn e2e` still to be run against a live stack (gates: `nav`/`theme`/`command-palette`).

### Track 5 — forms
- ✅ **Phase 16 — build `hc-date-picker` + regroup the patient form**. `hc-date-picker` (`ui/date-picker/`) blueprinted from the shadcn **date-picker-with-input** anatomy: the text input stays the primary control and the calendar is an affordance beside it. **This split was the phase's main design call** — a calendar-only trigger makes a birthdate slow to enter (paging month-by-month through decades) and would have broken all six e2e `.fill('YYYY-MM-DD')` call sites; keeping the input means `patient-dob-input` keeps its id + testid and every existing caller drives it unchanged. Value is an ISO `YYYY-MM-DD` string exposed via **ControlValueAccessor**, so consumers just bind `formControlName`. The popover leads with **month + year selects** (year descending, 120-year span) for the same birthdate reason; `max` disables later days. **All date math runs on Y/M/D parts, never an instant** — `new Date('1990-01-01')` parses as UTC midnight and reads back as Dec 31 in any negative-offset zone (this dev box is UTC−3, so the regression test has teeth here; `TZ` is unset, so it would be vacuous on a UTC CI box). a11y = dialog-plus-grid with focus on the container and `aria-activedescendant` tracking the active day, so the highlight is painted explicitly (as `hc-command` does); arrows move, Enter selects, Esc/click-outside close. Added the `calendar` icon. Patient form split into **Demographics + Contact `<fieldset>`s** (real accessible group names, chrome reset) divided by an `hc-separator`; DOB → `hc-date-picker` bounded at today. **Gender was already on `hc-select`** — that third of the phase needed no change, and a characterization spec now pins it. No testid renamed. Suite **243 → 264** (+21); build clean. New e2e calendar-pick flow in `patients.spec.ts`; gate `patients.spec.ts`.

**Track 5 complete** (committed straight to `main`). Net: +1 primitive (`hc-date-picker`), +1 icon (`calendar`). Vitest **243 → 264** (+21). Build clean. **All 5 tracks of the plan are now done.**

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
- **Built** `hc-sheet` (Phase 10): right-anchored `<dialog>`-based slide-over; `open` model, `ariaLabel`, GSAP slide-in behind reduced-motion.
- **Extended** `PatientDetailComponent` with an optional `patientId` input (Phase 11) so it can render inside the search slide-over; anonymize confirm → `hc-dialog`.
- **Adopted** `hc-sheet` in the patients search page (Phase 11): row-click / row-action View open the detail without navigating (URL stays `/patients`).
- **Adopted** `hc-dropdown-menu` + `hc-dialog` in the order-detail exam-item actions (Phase 12): row actions in a dropdown, reject/on-hold reason capture in dialogs.
- **Fixed** `hc-dropdown-menu` styling (pre-existing, Phase 5 regression): item rules moved behind `:host ::ng-deep` — see "Content projection gotcha" below.
- **Built** `hc-avatar` (Phase 13): `name`/`src`/`size`/`testId`; initials fallback, image-error fallback tracked by src value.
- **Adopted** `hc-avatar` + `hc-dropdown-menu` in the shell footer (Phase 13): theme toggle, sign out and role badge consolidated into one user menu.
- **Built** `hc-breadcrumb` (Phase 14): `items`/`testId`/`ariaLabel`; `nav[aria-label]` host landmark, last crumb `aria-current="page"` and never linked.
- **Adopted** `hc-breadcrumb` in order-detail + patient-detail (Phase 14); routed page only for patients (sheet mode has no trail).
- **Built** `hc-command` (Phase 15): `open`/`query` models, `items`/`placeholder`/`emptyMessage`/`ariaLabel`/`testId` inputs, `select` output; native `<dialog>`, combobox + `aria-activedescendant` listbox, groups, no internal filtering.
- **Adopted** `hc-command` in the shell (Phase 15): Ctrl/Cmd-K palette over role-aware nav destinations + debounced patient matches.
- **Extended** `PatientsService` with `quickSearch(term)` (Phase 15) — returns results without touching the page-level `searchResults`/`searching` signals.
- **Extended** `hc-sheet` + `hc-dialog` with a `testId` input bound to the **`<dialog>`** (2026-07-16): the host box is 0×0, so a testid there is never visible to a browser-driven test. Consumers pass `testId="…"` instead of `data-testid="…"` — **same values, no testid renamed** (`patient-detail-sheet`, `create-user-dialog`, `role-change-dialog`, `anonymize-dialog`, `reject-dialog`, `on-hold-dialog`).
- **Fixed** `hc-command` (2026-07-16): the top-match reset keys off item **content**, not array identity — an equivalent result set no longer clobbers the user's arrow-key selection.
- **Fixed** `UserListComponent.confirmRoleChange` (2026-07-16): catches the API rejection and shows `role-change-error-toast` naming the reason. Previously the 409 was swallowed silently.
- **Built** `hc-date-picker` (Phase 16): `testId`/`inputId`/`inputTestId`/`max`/`invalid`/`placeholder`/`ariaLabel` inputs; ISO `YYYY-MM-DD` value via ControlValueAccessor; text-input-primary with a month/year-led calendar popover; part-based date math (no UTC drift).
- **Extended** `hc-icon` with `calendar` (Phase 16).
- **Adopted** `hc-date-picker` + `hc-separator` in the patient form (Phase 16); fields grouped into Demographics + Contact fieldsets, DOB bounded at today.

## Content projection gotcha (bug found & fixed in Track 4)

A primitive **cannot style consumer-authored elements it projects** with plain scoped selectors. Under emulated encapsulation, projected nodes carry the **consumer's** `_ngcontent-*` attribute, so `dropdown-menu.css`'s `.hc-dropdown__item` emitted as `.hc-dropdown__item[_ngcontent-<dropdown>]` and **never matched** — every row-action menu from Phases 6/7/8/9/12 was rendering as **unstyled native buttons**.

Nothing caught it: Vitest asserts roles/behaviour rather than computed style, axe passes unstyled buttons, and full e2e has never run against a live stack. Confirmed by grepping the built CSS (`card.css`, which already uses `:host ::ng-deep` for its projected parts, emits **without** a content attribute).

**Rule:** style projected parts via `:host ::ng-deep .part` (as `card.css`/`table.css` do). A class declared in the *consumer's* own template (e.g. the shell's `.logout-item`) is already in the consumer's scope and needs no `::ng-deep`. Regression test: `dropdown-menu.spec.ts` → "styles content-projected menu items" asserts `display:flex` (a non-token property jsdom can resolve).

## Open follow-ups / notes

- ~~Full **e2e has still not been run**~~ — **RESOLVED 2026-07-16.** The stack came up (Docker was available again) and the suite ran on chromium: 102 passed / 7 failed / 5 skipped, five root causes, all fixed — see the table at the top. Remaining e2e caveats:
  - **Only chromium has been run.** `firefox` and `webkit` are still unexercised; `playwright.config.ts` defines all three. The cookie `Secure` flag fix (memory `project_e2e_infra`) exists specifically for WebKit, so that path deserves a run.
  - `reporter: 'html'` auto-opens a browser and blocks a non-interactive run — pass `--reporter=line` when running from a script/agent.
  - 5 tests remain **skipped** (seed-data-gated: the worklist sign-report and triage full-pipeline flows need cross-module events to have flowed).
- Phase 16's styling was checked against the Track-4 gotcha *without* a live stack, by grepping the built CSS: `.hc-date-picker__*` rules emit as `.hc-date-picker__day[_ngcontent-%COMP%]` and every element they target is authored in `date-picker.html` (nothing is projected), so they match; `.hc-input`/`.hc-select` emit from the global `ui.css` bundle with **no** content attribute, so they style the picker's own input/selects. This rules out that specific bug class but is **not** a substitute for seeing it render.
- The date-picker's timezone regression test is only meaningful in a negative-offset zone. `TZ` is unset in the Vitest config, so it passes vacuously on a UTC CI box — **pin `TZ` in the test config** if the suite ever runs in CI.
- The **shadcn MCP server was not connected** during Track 4; the three primitives were blueprinted from the documented shadcn anatomy/a11y contracts instead (the same contracts the MCP returns). Re-run `mcp__shadcn__get_audit_checklist` against `avatar`/`breadcrumb`/`command` if the server comes back.
- **Phase 15 scope call:** the plan's "jump-to-order by ID" was left out — orders are identified by UUID, which nobody types, and the orders list is itself a palette destination. Patient jump was kept (it is the "unify the scattered search inputs" value). Revisit if orders gain a human-readable accession number.
- **Track 1 (Phases 1–3b) merged to `main`** on 2026-07-13 via `--no-ff` per-phase merges (commits `b810a64` P1, `45f6842` P2, `20aef05` P3, `ee54cde` P3b). The Phase 3 ↔ 3b overlap on `order-list.component.*` was resolved to the union (skeleton branch + `hc-empty` empty branch). Post-merge baseline on `main`: **158 Vitest tests green**, build clean.
- This tracker now lives on `main`; Track 2+ branches inherit it. **Track 2 develops on a single `feat/frontend-track-2` branch off `main`** (its phases stack: the 4 tables consume the Phase 4/5 primitives), one commit-pair per phase.
- Memory: `project_frontend_shadcn_ux.md` mirrors this status for cross-session continuity.
