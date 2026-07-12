# Frontend Refactor — Handoff

**Date:** 2026-07-12
**Status:** Approved plan, not yet started
**Executing session:** start here. Read this document + root `CLAUDE.md` before any code change.

---

## 1. Context

The HC.LIS SPA (`src/HC.LIS.Frontend/packages/hc-lis-spa`) is an Angular 21.2 standalone +
signals app (Yarn classic workspaces, Vite/esbuild builder via `@angular/build:application`).
It works, but the UI is bespoke: no shared component library, per-component hand-written CSS
(mix of inline `styles:` blocks and external `.css`), inline hand-authored SVG icons, and
near-zero animation (one login spinner + CSS hover transitions).

A dark/light theme system was recently added and **must be preserved**:

| Seam | Owner file |
|---|---|
| Storage + DOM mutation (`data-theme` attr on `<html>`, localStorage `hc-lis-theme`) | `src/app/core/application/theme.service.ts` |
| Token overrides | `[data-theme="dark"]` block in `src/styles.css` |
| Toggle UI (`data-testid="theme-toggle-btn"`, sidebar footer) | `src/app/core/shell/shell.component.*` |
| Design spec | `docs/superpowers/specs/2026-07-09-theme-system-design.md` |

`ThemeService.init()` runs in the `App` constructor (`src/app/app.ts`) to prevent a
flash-of-wrong-theme — do not move it.

**Goal:** modern, pixel-perfect healthcare frontend — Design System v2 tokens, a shadcn-style
Angular component library in `src/app/ui/`, subtle GSAP motion, full e2e coverage. Full app,
phased; each phase gated by its e2e specs.

### User-confirmed decisions (do not re-litigate)

1. **Components:** hand-rolled shadcn-style Angular standalone components in `src/app/ui/`,
   blueprinted from the shadcn registry via shadcn MCP. **No Tailwind, no spartan/ng, no
   Angular Material.** Style exclusively with CSS custom-property tokens.
2. **Identity:** healthcare cyan/green — primary `#0891B2`, CTA green `#059669`, Figtree
   (headings) + Noto Sans (body), keep JetBrains Mono for barcodes/mono data. Archetype:
   **"Accessible & Ethical"** (WCAG AA minimum, AAA where cheap; light + dark tuned
   independently).
3. **Scope:** entire app, in phases (see §5).

---

## 2. Skill / tool invocation map (MANDATORY)

Invoke these via the Skill tool / MCP at the phase where they appear — do not work from memory:

| Skill / tool | When |
|---|---|
| `ui-ux-pro-max:ui-ux-pro-max` | Phase 0.1 token disputes (`--domain color/typography`), acceptance criteria (§1 a11y, §2 touch, §8 forms, §9 nav, §10 tables), Phase 4 pre-delivery checklist. Script: `python <plugin>/scripts/search.py "<query>" --design-system --persist -p "HealthCore LIS"` (optional MASTER.md persist — hand-edit to match §3 below) |
| `gsap-skills:gsap-core` | Phase 0.3 foundation (matchMedia, autoAlpha, immediateRender), Phase 2 route crossfade |
| `gsap-skills:gsap-frameworks` | Phase 0.3 — Angular lifecycle == the Vue/Svelte pattern: create in `ngAfterViewInit` inside `gsap.context(cb, scopeEl)`, `ctx.revert()` in `ngOnDestroy` |
| `gsap-skills:gsap-timeline` | Phase 3 dialog/toast sequences |
| `gsap-skills:gsap-performance` | Phase 4 audit |
| shadcn MCP: `mcp__shadcn__view_items_in_registries`, `mcp__shadcn__get_item_examples_from_registries`, `mcp__shadcn__get_audit_checklist` | Phase 1 — pull each primitive's blueprint (anatomy, variants, states, ARIA) before building it; audit checklist at phase end |
| `playwright-cli` project skill + Playwright MCP browser tools | All e2e authoring + visual verification in BOTH themes |
| `superpowers:test-driven-development`, `superpowers:executing-plans` | Process discipline throughout |

---

## 3. Design System v2 — token source of truth

Generated via `ui-ux-pro-max --design-system` ("healthcare medical laboratory clinical
dashboard", dials: **motion 3/10, density 8/10**).
**Anti-patterns:** no neon, no purple/pink gradients, no motion-heavy choreography, no emoji icons.

**Token strategy: keep existing token NAMES, change VALUES** (component CSS already consumes
`var(--color-*)` everywhere). Add new tokens only for missing concepts. Single seam:
`packages/hc-lis-spa/src/styles.css`.

### Light theme (`:root`)

| Token | Value | Note |
|---|---|---|
| `--color-bg` | `#F8FAFC` | |
| `--color-surface` | `#FFFFFF` | |
| `--color-surface-2` | `#F1F5F9` | |
| `--color-border` / `--color-border-strong` | rederive from slate to match | keep visible in BOTH themes |
| `--color-accent` | `#0891B2` | cyan-600; 4.6:1 on white ✓ |
| `--color-accent-light` | `#ECFEFF` | |
| `--color-accent-mid` | `#06B6D4` | |
| `--color-cta` **(NEW)** | `#059669` | health green — primary confirm/CTA |
| `--color-cta-bg` **(NEW)** | `#ECFDF5` | |
| `--color-text` | `#0F2E3D` | darkened for AAA |
| `--color-text-muted` | `#526B78` | ≥4.5:1 on bg/surface |
| `--color-text-subtle` | keep, but use for large/secondary text only | |
| `--color-error/warning/success` (+`-bg`) | keep hues; **verify** each fg on its `*-bg` pair ≥4.5:1 | |

### Dark theme (`[data-theme="dark"]`) — desaturated/lightened variants, never inverted

| Token | Value | Note |
|---|---|---|
| Surfaces | keep existing slate family (`#0F172A` bg / `#1E293B` surface / `#334155` surface-2) | already works |
| `--color-accent` | `#22D3EE` | cyan-400, ≥8:1 on dark bg |
| `--color-cta` | `#34D399` | |
| `--color-error` **(NEW override)** | `#F87171` | semantic tokens are currently UNthemed — known contrast gap |
| `--color-warning` **(NEW override)** | `#FBBF24` | |
| `--color-success` **(NEW override)** | `#4ADE80` | |
| `*-bg` variants **(NEW overrides)** | dark equivalents, e.g. `--color-error-bg: #450A0A` | |
| `--color-role-*` **(NEW overrides)** | dark variants — current values (`#0369a1` etc.) fail on dark surfaces | |

Contrast rule for every pair, both themes: **compute the ratio, don't eyeball** — ≥4.5:1 normal
text, ≥3:1 large text and UI glyphs. The `a11y.spec.ts` axe scan (Phase 0.2) is the automated gate.

### Typography

Swap the Google Fonts import in `src/index.html` and `src/styles.css` to
`Figtree:wght@400;500;600;700` (headings) + `Noto Sans:wght@400;500;700` (body); keep JetBrains
Mono. Add a `tabular-nums` utility (`font-variant-numeric: tabular-nums`) for data tables/timers.

### Motion tokens (NEW in `:root`)

`--motion-fast: 150ms`, `--motion-normal: 200ms`, `--motion-slow: 300ms`.
Rules: micro-interactions 150–300ms; exits ~60–70% of enter; transform/opacity only (GSAP
`autoAlpha`, never `width/height/top/left`); stagger 30–50ms/item; eases `power1.out`/`power2.out`;
**everything** behind `prefers-reduced-motion` (GSAP `matchMedia` + a global CSS
`@media (prefers-reduced-motion: reduce)` guard).

---

## 4. Hard constraints

- **TDD (CLAUDE.md):** failing Playwright test BEFORE any Angular component; `test:` commit
  precedes `feat:` commit. Vitest unit tests first for non-routable `ui/` primitives.
- **Do NOT rename existing `data-testid`s** — 152 in use across 8 specs; they are the regression
  net. Notables: `theme-toggle-btn`, `nav-*-link`, `worklist-row`, `refresh-btn`, `users-title`.
- Playwright selectors: `data-testid` kebab-case (or role/label) — never CSS class/tag.
- Seed users: `root@hclis.local` (only migration-guaranteed one), `receptionist@`, `labtech@`,
  `physician@` — all `@hclis.local` / `Admin1234!`.
- `angular.json` style budget: 4kB warn / 8kB error per component — keep primitive CSS lean.
- Keep the `body.printing-labels` print-isolation block in `styles.css` (label modal printing).
- Keep hexagonal boundaries: components never import `@hc-lis/api-client` directly; SDK stays
  behind `core/infrastructure/*` adapters and DI ports in `app.config.ts`.

---

## 5. Phases

Per-task loop: **(a)** failing spec (`test:` commit) → **(b)** implement (`feat:` commit) →
**(c)** `yarn e2e` (chromium minimum) + Playwright MCP visual check in BOTH themes.

### Phase 0 — Foundations
1. **Tokens v2** — rewrite `:root` + `[data-theme="dark"]` in `src/styles.css` per §3; swap
   fonts in `index.html`; add motion tokens, `tabular-nums` utility, reduced-motion CSS guard.
   Do not touch the ThemeService seams.
2. **E2E infra** — extract duplicated `loginAs*` helpers from all 8 specs into
   `e2e/fixtures/auth.ts`; add `e2e/theme.spec.ts` (toggle switches `data-theme`, persists
   across reload, icon swap, works per role); add `@axe-core/playwright` +
   `e2e/a11y.spec.ts` scanning every route in both themes.
3. **GSAP foundation** — `yarn add gsap`; `src/app/ui/motion/motion.ts`:
   `gsap.defaults({ duration: .2, ease: "power2.out" })`, `gsap.matchMedia()` wrapper with
   `reduceMotion` condition (skip/duration-0), Angular helper (`ngAfterViewInit` +
   `gsap.context` scoped to the host element, `ctx.revert()` on destroy). Vitest tests.

### Phase 1 — `src/app/ui/` component library
Build each primitive from its shadcn blueprint (`hc-` selector prefix, signal inputs, external
`.html`/`.css`, tokens only, `data-testid` passthrough):

| Primitive | Replaces / used by |
|---|---|
| `button` (default/cta/ghost/destructive, loading) | all forms, `refresh-btn`, logout, theme toggle |
| `input` + `label` + `field` (helper/error text, validate on blur) | login, patient-form, create-user-form, request-exam-form |
| `select` / `native-select` | forms |
| `badge` (role + status variants) | shell role badge, order/worklist statuses |
| `card` | triage cards, detail panels, print-labels-card |
| `table` (sticky header, tabular-nums, `aria-sort`, dense) | order-list, worklist, user-list, patient-search |
| `dialog` (focus trap, Esc, scrim 40–60%) | print-labels-modal |
| `combobox` | patient-picker typeahead |
| `tabs` | patient-detail / order-detail sections |
| `alert` + toast (sonner-style; `aria-live="polite"`, 3–5s) | error/success feedback |
| `skeleton`, `spinner` (>300ms rule) | loading states; replaces login `@keyframes spin` |
| `tooltip`, `separator`, `empty` | icon-only buttons, empty lists |
| `icon` (consolidate inline SVGs, Lucide-style, 1.5px stroke) | shell nav, theme toggle, features |

End of phase: run `mcp__shadcn__get_audit_checklist`.

### Phase 2 — Shell & navigation
E2E first (active-state spec + existing nav assertions), then restyle
`core/shell/shell.component.*` with `ui/` primitives: nav hierarchy + active indicator,
icon+label items, `hc-badge` role badge, footer tooltips. Motion: route-change crossfade on a
`<router-outlet>` wrapper (autoAlpha, 200ms in / 120ms out, `power1.inOut`, never blocks
navigation) via Router events inside the matchMedia wrapper. Sidebar hover stays CSS-only.

### Phase 3 — Page migrations (one PR-sized slice each)
Order: **auth/login → patients (search/register/detail) → orders (list/new/detail +
patient-picker) → triage (+ print-labels modal) → worklist (+ item detail) → admin/users →
unauthorized/not-found**.

Per page: existing spec stays green throughout → add missing coverage (happy-path workflow +
≥1 role-guard test; note `worklist.spec.ts` has a `test.fixme` full sign-report workflow needing
full pipeline seed data) → migrate template to `ui/` primitives, move inline `styles:` to
external `.css` → motion: row entrance stagger (≤20 rows, 30ms, y:8 + autoAlpha, once per
load), dialog scale 0.98→1 (180ms/120ms), toast slide-in — all through the Phase 0.3 helper.

### Phase 4 — Polish & audit gate
- `gsap-performance` pass; reduced-motion e2e via `emulateMedia({ reducedMotion: 'reduce' })`.
- `ui-ux-pro-max` pre-delivery checklist (§1–§3), 375px viewport, both-theme axe/contrast audit.
- Full suite: `yarn e2e` chromium+firefox+webkit, `yarn test`, `yarn build`.
- Update `docs/specs/HealthcoreSPA-TechSpec.md` with the `ui/` layer + Design System v2.

---

## 6. Verification checklist (final)

1. `cd src/HC.LIS.Frontend && yarn install && yarn workspace hc-lis-spa build` — clean.
2. Backend: `docker-compose -f development-compose.yaml up -d` → run migrator
   (`dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj`) → API up → `ng serve`;
   then `yarn e2e` green in all 3 browsers incl. `theme.spec.ts` + `a11y.spec.ts`.
   (E2E env details: memory `project_e2e_infra`; base URL override `E2E_BASE_URL`.)
3. Playwright MCP sweep: screenshot every route in light AND dark; no flash-of-wrong-theme on
   reload.
4. Reduced-motion path verified; nothing animates >300ms; no layout-property tweens.

## 7. Reference inventory (as of 2026-07-12)

- Routes/components: login, orders (list/new/detail), triage (`/waiting-room` redirects),
  worklist, admin/users, patients (search/new/detail), unauthorized, not-found — all lazy
  `loadComponent` in `src/app/app.routes.ts`, `authGuard` + `roleGuard` protected.
- E2E specs (8): `auth` (7 tests), `admin-users` (5), `patients` (4), `order-patient-picker` (3),
  `orders` (14), `triage` (8), `worklist` (3, one fixme), `hipaa` (5 — no PHI in URLs; keep
  passing). Config: 1 worker, 3 browsers, `E2E_BASE_URL` or `localhost:4200`.
- SDK: `@hc-lis/api-client` (`packages/hc-lis-api-client`, @hey-api/openapi-ts), consumed only
  via `core/infrastructure/*` adapters.
- Related docs: `docs/prd/HealthcoreSPA.md`, `docs/specs/HealthcoreSPA-TechSpec.md`,
  `docs/specs/PatientManagementUI-TechSpec.md`, theme spec (§1 above).
