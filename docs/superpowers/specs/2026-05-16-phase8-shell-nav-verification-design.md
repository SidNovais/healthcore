# Phase 8 — Shell Navigation + Final Verification

**Date:** 2026-05-16  
**Scope:** F-17 (Orders nav link in ShellComponent) + V-1 (backend tests) + V-2 (E2E suite)

---

## Context

Phases 1–7 delivered the complete TestOrders lifecycle: backend queries, API endpoints, and the full
SPA workflow (order list, order detail, all exam lifecycle actions). The SPA is now functional but
the Orders section is unreachable from the main navigation — users must navigate directly by URL.

Phase 8 closes that gap by adding the Orders nav link to the shell, then runs full verification to
confirm the entire TestOrders feature ships cleanly.

---

## Architecture

No new files. All changes are confined to existing files:

| File | Change |
|------|--------|
| `e2e/auth.spec.ts` | Add nav visibility tests (E2E-first) |
| `core/shell/shell.component.ts` | Add `testId` to `NavItem`; extend `NAV_BY_ROLE` |
| `core/shell/shell.component.html` | Bind `[attr.data-testid]` on nav `<a>`; add `'list'` icon case |

---

## F-17: Orders Nav Link

### NavItem interface change

Add a `testId` field to `NavItem` so each link carries its own testable identifier:

```typescript
interface NavItem {
  label: string;
  route: string;
  icon: string;
  testId: string;
}
```

### NAV_BY_ROLE update

Add an Orders entry to Receptionist, Physician, and ITAdmin:

```typescript
const NAV_BY_ROLE: Record<UserRole, NavItem[]> = {
  Receptionist: [
    { label: 'New Order', route: '/orders/new', icon: 'order', testId: 'nav-new-order-link' },
    { label: 'Orders',    route: '/orders',     icon: 'list',  testId: 'nav-orders-link'   },
  ],
  LabTechnician: [
    { label: 'Waiting Room', route: '/waiting-room', icon: 'queue', testId: 'nav-waiting-room-link' },
  ],
  Physician: [
    { label: 'Orders',   route: '/orders',   icon: 'list',     testId: 'nav-orders-link'  },
    { label: 'Worklist', route: '/worklist', icon: 'worklist', testId: 'nav-worklist-link' },
  ],
  ITAdmin: [
    { label: 'New Order',    route: '/orders/new',   icon: 'order',    testId: 'nav-new-order-link'    },
    { label: 'Orders',       route: '/orders',        icon: 'list',     testId: 'nav-orders-link'       },
    { label: 'Waiting Room', route: '/waiting-room',  icon: 'queue',    testId: 'nav-waiting-room-link' },
    { label: 'Worklist',     route: '/worklist',      icon: 'worklist', testId: 'nav-worklist-link'     },
    { label: 'Users',        route: '/admin/users',   icon: 'users',    testId: 'nav-users-link'        },
  ],
};
```

### Template change

Bind `data-testid` on each nav anchor and add a `'list'` icon case:

```html
<a
  [routerLink]="item.route"
  routerLinkActive="active"
  class="nav-item"
  [attr.aria-label]="item.label"
  [attr.data-testid]="item.testId"
>
  ...
  @case ('list') {
    <svg ...><!-- lines / list icon --></svg>
  }
```

---

## E2E Tests (TDD-first — written before implementation)

Added to `e2e/auth.spec.ts` under a "Navigation" describe block:

| Test | Assertion |
|------|-----------|
| Receptionist sees Orders link | `nav-orders-link` is visible after login |
| Physician sees Orders link | `nav-orders-link` is visible after login |
| ITAdmin sees Orders link | `nav-orders-link` is visible after login |
| LabTechnician does NOT see Orders link | `nav-orders-link` is not present |

Each test logs in via role helper, waits for the nav to render, then asserts visibility.

---

## Verification (V-1 + V-2)

### V-1 — Backend tests

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/HC.LIS.Modules.TestOrders.UnitTests.csproj
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/ArchTests/HC.LIS.Modules.TestOrders.ArchTests.csproj
```

All three must pass with 0 failures.

### V-2 — E2E suite

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn e2e
```

Requires API + DB running. All specs must pass (non-fixme tests).

---

## Success Criteria

- `[data-testid="nav-orders-link"]` is visible for Receptionist, Physician, ITAdmin after login
- `[data-testid="nav-orders-link"]` is absent for LabTechnician
- Clicking the link navigates to `/orders` and shows the order list
- All three TestOrders .NET test projects pass
- Full Playwright E2E suite passes (non-fixme tests)
