# Phase 8 — Shell Navigation + Final Verification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the Orders nav link to ShellComponent for Receptionist, Physician, and ITAdmin, then confirm the entire TestOrders feature passes all tests.

**Architecture:** `NavItem` gains a `testId` field so each nav anchor gets a unique `data-testid`; `NAV_BY_ROLE` is extended with the Orders entry for the three roles; the HTML template binds the new field and adds a `'list'` icon case. Verification runs all three TestOrders .NET projects and the full Playwright E2E suite.

**Tech Stack:** Angular 17 (signals, standalone components), Playwright, xUnit / FluentAssertions / NetArchTest.Rules

---

## File Map

| File | Change |
|------|--------|
| `e2e/auth.spec.ts` | Add `Shell Navigation` describe block with 4 nav-visibility tests |
| `src/app/core/shell/shell.component.ts` | Add `testId` to `NavItem`; extend `NAV_BY_ROLE` |
| `src/app/core/shell/shell.component.html` | Bind `[attr.data-testid]`; add `@case ('list')` icon |
| `docs/plans/test-orders-lifecycle.md` | Mark F-17, V-1, V-2 complete |

All paths below are relative to `src/HC.LIS.Frontend/packages/hc-lis-spa/`.

---

## Task 1 — Write failing E2E tests for Orders nav link

**Files:**
- Modify: `e2e/auth.spec.ts`

- [ ] **Step 1: Append the Shell Navigation describe block to `e2e/auth.spec.ts`**

Add the following at the end of the file (after the closing `}`  of the Authentication describe block):

```typescript
const RECEPTIONIST_EMAIL = 'receptionist@hclis.local';
const RECEPTIONIST_PASSWORD = 'Admin1234!';
const PHYSICIAN_EMAIL = 'physician@hclis.local';
const PHYSICIAN_PASSWORD = 'Admin1234!';
const LAB_TECH_EMAIL = 'labtech@hclis.local';
const LAB_TECH_PASSWORD = 'Admin1234!';

async function loginAsReceptionist(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(RECEPTIONIST_EMAIL);
  await page.getByLabel('Password').fill(RECEPTIONIST_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/orders/new', { timeout: 10_000 });
}

async function loginAsPhysician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(PHYSICIAN_EMAIL);
  await page.getByLabel('Password').fill(PHYSICIAN_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/worklist', { timeout: 10_000 });
}

async function loginAsLabTechnician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(LAB_TECH_EMAIL);
  await page.getByLabel('Password').fill(LAB_TECH_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/waiting-room', { timeout: 10_000 });
}

test.describe('Shell Navigation', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist sees Orders nav link', async ({ page }) => {
    await loginAsReceptionist(page);
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('Physician sees Orders nav link', async ({ page }) => {
    await loginAsPhysician(page);
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('ITAdmin sees Orders nav link', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill(ROOT_EMAIL);
    await page.getByLabel('Password').fill(ROOT_PASSWORD);
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/admin/users', { timeout: 10_000 });
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('LabTechnician does not see Orders nav link', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('nav-orders-link')).not.toBeVisible({ timeout: 5_000 });
  });
});
```

- [ ] **Step 2: Run the new tests — confirm they fail**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn playwright test e2e/auth.spec.ts --grep "Shell Navigation"
```

Expected: All 4 tests FAIL. Playwright reports something like:
```
Error: locator.toBeVisible: Error: strict mode violation: getByTestId('nav-orders-link') ...
```
(The element doesn't exist yet because `data-testid` isn't bound in the template.)

- [ ] **Step 3: Commit the failing tests**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/auth.spec.ts
git commit -m "test(spa): add failing E2E tests for Orders nav link visibility (F-17)"
```

> All `git` commands run from the **repo root** (`C:\Users\sidne\Development\healthcore\`), not from the SPA directory.

---

## Task 2 — Implement ShellComponent: testId + Orders nav entries + list icon

**Files:**
- Modify: `src/app/core/shell/shell.component.ts`
- Modify: `src/app/core/shell/shell.component.html`

- [ ] **Step 1: Update `shell.component.ts` — add `testId` field and extend `NAV_BY_ROLE`**

Replace the entire file content with:

```typescript
import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../application/auth.service';
import type { UserRole } from '../domain/user-session';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  testId: string;
}

const NAV_BY_ROLE: Record<UserRole, NavItem[]> = {
  Receptionist: [
    { label: 'New Order', route: '/orders/new', icon: 'order', testId: 'nav-new-order-link' },
    { label: 'Orders',    route: '/orders',     icon: 'list',  testId: 'nav-orders-link'    },
  ],
  LabTechnician: [
    { label: 'Waiting Room', route: '/waiting-room', icon: 'queue', testId: 'nav-waiting-room-link' },
  ],
  Physician: [
    { label: 'Orders',   route: '/orders',   icon: 'list',     testId: 'nav-orders-link'   },
    { label: 'Worklist', route: '/worklist', icon: 'worklist', testId: 'nav-worklist-link'  },
  ],
  ITAdmin: [
    { label: 'New Order',    route: '/orders/new',  icon: 'order',    testId: 'nav-new-order-link'    },
    { label: 'Orders',       route: '/orders',       icon: 'list',     testId: 'nav-orders-link'       },
    { label: 'Waiting Room', route: '/waiting-room', icon: 'queue',    testId: 'nav-waiting-room-link' },
    { label: 'Worklist',     route: '/worklist',     icon: 'worklist', testId: 'nav-worklist-link'     },
    { label: 'Users',        route: '/admin/users',  icon: 'users',    testId: 'nav-users-link'        },
  ],
};

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.authService.currentUser;

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.user()?.role;
    return role ? (NAV_BY_ROLE[role] ?? []) : [];
  });

  async logout(): Promise<void> {
    await this.authService.logout();
    await this.router.navigate(['/login']);
  }
}
```

- [ ] **Step 2: Update `shell.component.html` — bind `data-testid` and add `list` icon case**

In the `<a>` element inside `@for`, add `[attr.data-testid]="item.testId"`:

```html
<a
  [routerLink]="item.route"
  routerLinkActive="active"
  class="nav-item"
  [attr.aria-label]="item.label"
  [attr.data-testid]="item.testId"
>
```

Inside the `@switch (item.icon)` block, add a `@case ('list')` **before** the closing `}` of the switch. Use the same list-with-dots SVG style as the worklist icon — both represent a list and the label differentiates them visually:

```html
@case ('list') {
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <rect x="3" y="5" width="18" height="14" rx="2"/><line x1="3" y1="10" x2="21" y2="10"/><line x1="3" y1="15" x2="21" y2="15"/><line x1="8" y1="10" x2="8" y2="19"/>
  </svg>
}
```

- [ ] **Step 3: Build to confirm no TypeScript errors**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
npx ng build --configuration development 2>&1 | tail -5
```

Expected: `Build at: ... - Hash: ... - Time: ...ms` with no errors.

- [ ] **Step 4: Commit the implementation**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/shell/shell.component.ts
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/shell/shell.component.html
git commit -m "feat(spa): add Orders nav link to ShellComponent for Receptionist, Physician, ITAdmin (F-17)"
```

---

## Task 3 — Verify nav E2E tests pass

- [ ] **Step 1: Run the Shell Navigation tests**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn playwright test e2e/auth.spec.ts --grep "Shell Navigation"
```

Expected: All 4 tests PASS.

```
✓ Shell Navigation › Receptionist sees Orders nav link
✓ Shell Navigation › Physician sees Orders nav link
✓ Shell Navigation › ITAdmin sees Orders nav link
✓ Shell Navigation › LabTechnician does not see Orders nav link
```

If any test fails, diagnose before continuing. Common causes:
- Template change not saved — check `[attr.data-testid]` is on the `<a>`, not the `<span>`
- `NAV_BY_ROLE` entry missing — confirm `testId: 'nav-orders-link'` in Receptionist, Physician, ITAdmin arrays

---

## Task 4 — V-1: Backend test verification

- [ ] **Step 1: Run TestOrders Unit tests**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/HC.LIS.Modules.TestOrders.UnitTests.csproj
```

Expected: `Passed! - Failed: 0, Passed: N, Skipped: 0`

- [ ] **Step 2: Run TestOrders Integration tests**

Requires `ASPNETCORE_HCLIS_IntegrationTests_ConnectionString` env var pointing to a running Postgres instance.

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
```

Expected: `Passed! - Failed: 0, Passed: N, Skipped: 0`

- [ ] **Step 3: Run TestOrders Arch tests**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/ArchTests/HC.LIS.Modules.TestOrders.ArchTests.csproj
```

Expected: `Passed! - Failed: 0, Passed: N, Skipped: 0`

---

## Task 5 — V-2: Full E2E suite verification

- [ ] **Step 1: Ensure the stack is running**

The API + DB must be up. If not:
```bash
docker-compose -f development-compose.yaml up -d
```
Then start the API and the Angular dev server (`ng serve`) before running.

- [ ] **Step 2: Run the full E2E suite**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn e2e
```

Expected: All non-`fixme` tests pass. Tests marked `test.fixme(...)` are intentionally skipped — they require RabbitMQ + the full event pipeline; their skips are not failures.

---

## Task 6 — Mark tasks complete in master plan + commit

**Files:**
- Modify: `docs/plans/test-orders-lifecycle.md`

- [ ] **Step 1: Check off F-17, V-1, V-2 in the master plan**

In `docs/plans/test-orders-lifecycle.md`, change:

```markdown
- [ ] **F-17** Add Orders nav link to `ShellComponent` ...
```
to:
```markdown
- [x] **F-17** Add Orders nav link to `ShellComponent` ...
```

And:
```markdown
- [ ] **V-1** `dotnet test` ...
- [ ] **V-2** `yarn e2e` ...
```
to:
```markdown
- [x] **V-1** `dotnet test` ...
- [x] **V-2** `yarn e2e` ...
```

- [ ] **Step 2: Commit**

```bash
git add docs/plans/test-orders-lifecycle.md
git commit -m "docs(plans): mark phase 8 tasks complete (F-17, V-1, V-2)"
```
