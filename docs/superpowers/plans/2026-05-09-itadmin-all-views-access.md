# ITAdmin All-Views Access Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Grant the ITAdmin role unrestricted access to all feature views (New Order, Waiting Room, Worklist, Users) and reflect that in the shell navigation menu.

**Architecture:** The `roleGuard` factory in `role.guard.ts` already accepts variadic allowed roles — no guard logic changes are needed. The three protected routes must include `'ITAdmin'` alongside their primary role. The `NAV_BY_ROLE` map in `shell.component.ts` must list all four nav items for the ITAdmin entry.

**Tech Stack:** Angular 17+, Playwright E2E, TypeScript

---

## Files Modified

| File | Change |
|---|---|
| `e2e/admin-users.spec.ts` | Replace denial test with three "ITAdmin can access" tests |
| `src/app/app.routes.ts` | Add `'ITAdmin'` to `roleGuard()` for `/orders/new`, `/waiting-room`, `/worklist` |
| `src/app/core/shell/shell.component.ts` | Expand `NAV_BY_ROLE.ITAdmin` to all four nav items |

---

### Task 1: Write failing E2E tests (TDD)

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/admin-users.spec.ts`

- [x] **Step 1: Replace the denial test with three access tests**

In `admin-users.spec.ts`, remove the existing denial test (lines 21–25):
```typescript
  test('ITAdmin cannot access /worklist (role guard)', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/worklist');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
```

Add in its place — immediately after `'ITAdmin sees user list on login'`:
```typescript
  test('ITAdmin can access /orders/new', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/orders/new');
    await expect(page).toHaveURL('/orders/new', { timeout: 5_000 });
  });

  test('ITAdmin can access /waiting-room', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/waiting-room');
    await expect(page).toHaveURL('/waiting-room', { timeout: 5_000 });
  });

  test('ITAdmin can access /worklist', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/worklist');
    await expect(page).toHaveURL('/worklist', { timeout: 5_000 });
  });
```

The full updated `test.describe` block should look like:
```typescript
test.describe('User Management', () => {
  test('ITAdmin sees user list on login', async ({ page }) => {
    await loginAsITAdmin(page);
    await expect(page.getByTestId('users-title')).toBeVisible({ timeout: 5_000 });
  });

  test('ITAdmin can access /orders/new', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/orders/new');
    await expect(page).toHaveURL('/orders/new', { timeout: 5_000 });
  });

  test('ITAdmin can access /waiting-room', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/waiting-room');
    await expect(page).toHaveURL('/waiting-room', { timeout: 5_000 });
  });

  test('ITAdmin can access /worklist', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/worklist');
    await expect(page).toHaveURL('/worklist', { timeout: 5_000 });
  });

  test('full create-user workflow: open form → fill fields → submit → new user appears in list', async ({ page }) => {
    // ... unchanged
  });
});
```

- [x] **Step 2: Run the three new tests to confirm they fail**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn e2e --grep "ITAdmin can access"
```

Expected: All three tests **FAIL** — ITAdmin is redirected to `/unauthorized` because the guards don't yet include `'ITAdmin'`.

- [x] **Step 3: Commit the failing tests**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/admin-users.spec.ts
git commit -m "test(spa/itadmin): ITAdmin can access all feature views (failing)"
```

---

### Task 2: Update route guards

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/app.routes.ts`

- [x] **Step 1: Add `'ITAdmin'` to the three role guards**

In `app.routes.ts`, update each of the three protected routes:

```typescript
// Before:
{ path: 'orders/new', canActivate: [roleGuard('Receptionist')], ... }
{ path: 'waiting-room', canActivate: [roleGuard('LabTechnician')], ... }
{ path: 'worklist', canActivate: [roleGuard('Physician')], ... }

// After:
{ path: 'orders/new', canActivate: [roleGuard('Receptionist', 'ITAdmin')], ... }
{ path: 'waiting-room', canActivate: [roleGuard('LabTechnician', 'ITAdmin')], ... }
{ path: 'worklist', canActivate: [roleGuard('Physician', 'ITAdmin')], ... }
```

Full updated routes block:
```typescript
export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login.component').then(m => m.LoginComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'orders/new',
        canActivate: [roleGuard('Receptionist', 'ITAdmin')],
        loadComponent: () =>
          import('./features/orders/new-order.component').then(m => m.NewOrderComponent),
      },
      {
        path: 'waiting-room',
        canActivate: [roleGuard('LabTechnician', 'ITAdmin')],
        loadComponent: () =>
          import('./features/waiting-room/waiting-room.component').then(m => m.WaitingRoomComponent),
      },
      {
        path: 'worklist',
        canActivate: [roleGuard('Physician', 'ITAdmin')],
        loadComponent: () =>
          import('./features/worklist/worklist.component').then(m => m.WorklistComponent),
      },
      {
        path: 'admin/users',
        canActivate: [roleGuard('ITAdmin')],
        loadComponent: () =>
          import('./features/admin/user-list.component').then(m => m.UserListComponent),
      },
      {
        path: 'unauthorized',
        loadComponent: () =>
          import('./core/shell/unauthorized.component').then(m => m.UnauthorizedComponent),
      },
    ],
  },
  {
    path: '**',
    component: NotFoundComponent,
  },
];
```

- [x] **Step 2: Run the three E2E tests to confirm they now pass**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn e2e --grep "ITAdmin can access"
```

Expected: All three tests **PASS**.

- [x] **Step 3: Run the full E2E suite to confirm no regressions**

```bash
yarn e2e
```

Expected: All tests pass. The old denial test is gone; no other test asserts ITAdmin denial.

- [x] **Step 4: Commit the guard changes**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/app.routes.ts
git commit -m "feat(spa/itadmin): grant ITAdmin access to all feature views"
```

---

### Task 3: Update ITAdmin navigation menu

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/shell/shell.component.ts`

- [x] **Step 1: Expand `NAV_BY_ROLE.ITAdmin` to all four items**

In `shell.component.ts`, update the `ITAdmin` entry of `NAV_BY_ROLE`:

```typescript
// Before:
ITAdmin: [
  { label: 'Users', route: '/admin/users', icon: 'users' },
],

// After:
ITAdmin: [
  { label: 'New Order', route: '/orders/new', icon: 'order' },
  { label: 'Waiting Room', route: '/waiting-room', icon: 'queue' },
  { label: 'Worklist', route: '/worklist', icon: 'worklist' },
  { label: 'Users', route: '/admin/users', icon: 'users' },
],
```

- [x] **Step 2: Build the SPA to confirm no TypeScript errors**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn build
```

Expected: Build succeeds with no errors.

- [x] **Step 3: Start the dev server and manually verify the ITAdmin sidebar**

```bash
yarn start
```

Login as `itadmin@hclis.local` / `Admin1234!`. Verify the sidebar shows four items: **New Order**, **Waiting Room**, **Worklist**, **Users**. Click each and confirm navigation works without redirection to `/unauthorized`.

- [x] **Step 4: Commit**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/shell/shell.component.ts
git commit -m "feat(spa/itadmin): show all nav items in ITAdmin sidebar"
```

---

## Verification

Run the full E2E suite against a running dev server and API:

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn e2e
```

Check that:
- `admin-users.spec.ts` — all four tests in the describe block pass (including the three new access tests and the create-user workflow)
- All other spec files (`orders.spec.ts`, `waiting-room.spec.ts`, `worklist.spec.ts`, `auth.spec.ts`, `hipaa.spec.ts`) continue to pass without change
