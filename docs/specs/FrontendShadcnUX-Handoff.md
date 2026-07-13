# Frontend shadcn-UX Enhancement — Handoff & Progress Tracker

_Last updated: 2026-07-12_

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

Legend: ✅ done (branch, not merged) · 🔜 next · ⬜ planned

### Track 1 — adopt already-built primitives
- ✅ **Phase 1 — Triage** · branch `feat/frontend-phase-1-triage-adopt-primitives`
  Filter bar → `hc-tabs`; print-labels-modal → `hc-dialog` + `hc-skeleton`; empty divs → `hc-empty`. Build + 132 tests green.
- ✅ **Phase 2 — New-order** · branch `feat/frontend-phase-2-order-combobox-toast`
  patient-picker → `hc-combobox` (extended it with `inputTestId`/`listboxTestId`/`optionTestId` to preserve the picker's testids); exam confirmation → `hc-toast`. **`ToastService.show` gained an optional `testId` + dedupe; `hc-toaster` is now mounted at the app root — it was never mounted before, so no toast could render.** Build + 135 tests green.
- ✅ **Phase 3 — List-state consistency** · branch `feat/frontend-phase-3-list-states`
  order-list empty + order-detail empty-items → `hc-empty` (kept `empty-items` testid); order-detail Accept/Reject/Cancel/On-Hold → success toasts (error alert unchanged). Build + 133 tests green.
- ⬜ **Phase 3b — Loading skeletons** (carved out of the original Phase 3)
  Add a `loading` signal to OrdersService / patients / worklist / users services; show `hc-skeleton` rows while async tables/lists load (nothing uses skeletons today outside triage/modal). Deferred because it needs service changes and carries the most e2e-race risk — do it as its own focused phase. Gate each page's existing spec (they already `waitForResponse`).

### Track 2 — build the 2 missing primitives + upgrade the 4 tables
- 🔜 **Phase 4 — build `hc-pagination`** (`ui/pagination/`, blueprint shadcn `pagination`; nav/prev/next/pages, tabular-nums). Vitest-first.
- ⬜ **Phase 5 — build `hc-dropdown-menu`** (`ui/dropdown-menu/`) — hardest a11y: `role=menu/menuitem`, roving tabindex, arrow/Esc, click-outside, focus return. Vitest-first. (Candidate to reconsider `@spartan-ng/brain` if hand-rolled menu a11y proves costly — default is hand-rolled.)
- ⬜ **Phase 6 — Orders table**: pagination + sorting + row-action menu (96 testids — extreme care). Gate `orders.spec.ts`.
- ⬜ **Phase 7 — Patients table**: pagination + row actions + search-input polish (clear button, debounce, skeleton). Gate `patients.spec.ts`.
- ⬜ **Phase 8 — Worklist table**: pagination + sorting + row actions; keep inline detail (desktop-only). Gate `worklist.spec.ts`.
- ⬜ **Phase 9 — Admin/users table**: raw per-row role `<select>` → dropdown-menu + `hc-dialog` confirm-on-role-change **(+ the deferred role-change toast lives here)**; pagination; create-user form → dialog/sheet. Gate `admin-users.spec.ts`.

### Track 3 — detail slide-overs + order-detail actions
- ⬜ **Phase 10 — build `hc-sheet`** (extend `hc-dialog` focus-trap into a right-anchored slide-over).
- ⬜ **Phase 11 — Patient detail** as slide-over from patients search; anonymize confirm → `hc-dialog`. Gate `patients.spec.ts`, `hipaa.spec.ts`.
- ⬜ **Phase 12 — Order detail** reject/on-hold reason capture → `hc-dialog`; per-row action buttons → dropdown-menu. Gate `orders.spec.ts`.

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

## Open follow-ups / notes

- Full **e2e has not been run** during phase development (needs the API+DB+ng serve stack). Each phase is gated by build + the full Vitest suite + reasoning about the existing spec assertions; run `yarn e2e` per branch before merging.
- Phases 1–3 are on separate un-merged branches off `main`. Decide merge order / whether to stack them.
- Memory: `project_frontend_shadcn_ux.md` mirrors this status for cross-session continuity.
