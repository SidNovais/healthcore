# Phase 6 — Frontend: Order Detail Page

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `OrderDetailComponent` at `/orders/:id` — displays full exam item details and enables every exam lifecycle transition (Accept, Reject, Cancel, On Hold, In Progress, Partially Complete) with TDD.

**Architecture:** `OrderDetailComponent` is a standalone Angular component. It reads the `:id` param from `ActivatedRoute`, delegates to the already-existing `OrdersService` (signals: `orderDetails`; methods: `loadOrderDetails` + all lifecycle actions), and reloads the detail after every mutation. Actions requiring a reason (Reject, On Hold) reveal an inline form per item row. No new service, SDK, or backend changes are needed — Phase 4 built everything.

**Tech Stack:** Angular 21 / TypeScript 5.9 / Vitest + Angular TestBed for integration tests / Playwright for E2E.

---

## Files

### Modified
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/app.routes.ts` — add `orders/:id` route between `orders/new` and `orders`
- `src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/orders.spec.ts` — append Order Detail describe block

### Created
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.ts`
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.integration.spec.ts`

---

## Terms of Acceptance

All of the following must be true before Phase 6 is complete:

1. Clicking an order row in `/orders` navigates to `/orders/:id`.
2. `data-testid="order-detail"` container is present on the page.
3. `data-testid="exam-items-table"` is visible; one `data-testid="exam-item-row"` per exam item.
4. Each item shows its `ExamItemStatus` in `data-testid="item-status"`.
5. **Accept** — `data-testid="accept-btn"` on a `Requested` or `OnHold` item calls `acceptExam()`; status updates to `Accepted`.
6. **Reject with reason** — `data-testid="reject-btn"` on `Requested`/`Accepted` reveals `data-testid="reject-reason-form"`; confirm calls `rejectExam(orderId, itemId, reason)`; status updates to `Rejected`.
7. **Cancel** — available on non-terminal items; calls `cancelExam()`.
8. **On Hold with reason** — available on `Accepted` items; requires reason form; calls `placeExamOnHold()`.
9. **In Progress** — available on `Accepted` items; calls `placeExamInProgress()`.
10. **Partially Complete** — available on `InProgress` items; calls `partiallyCompleteExam()`.
11. `LabTechnician` navigating to `/orders/:id` is redirected to `/unauthorized`.
12. All existing unit + E2E tests continue to pass (no regressions).

---

## Task 1: Write failing E2E tests for Order Detail (F-12)

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/orders.spec.ts`

- [ ] **Step 1: Append the Order Detail describe block to `e2e/orders.spec.ts`**

  Add the following at the end of the file (after the closing `}` of `test.describe('Order List', ...)`):

  ```typescript
  const PHYSICIAN_EMAIL = 'physician@hclis.local';
  const PHYSICIAN_PASSWORD = 'Admin1234!';

  async function loginAsPhysician(page: import('@playwright/test').Page) {
    await page.goto('/login');
    await page.getByLabel('Email').fill(PHYSICIAN_EMAIL);
    await page.getByLabel('Password').fill(PHYSICIAN_PASSWORD);
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/worklist', { timeout: 10_000 });
  }

  test.describe('Order Detail', () => {
    test('Receptionist sees order detail page at /orders/:id', async ({ page }) => {
      await loginAsReceptionist(page);

      await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
      await page.getByTestId('create-order-btn').click();
      await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('exam-mnemonic-input').fill('GLU');
      await page.getByTestId('container-type-input').fill('RedTop');
      await page.getByTestId('request-exam-btn').click();
      await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      await page.goto('/orders');
      await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('order-list-row').first().click();
      await expect(page).toHaveURL(
        /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
        { timeout: 5_000 }
      );

      await expect(page.getByTestId('order-detail')).toBeVisible({ timeout: 5_000 });
    });

    test('Order detail page shows exam items table with one row', async ({ page }) => {
      await loginAsReceptionist(page);

      await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
      await page.getByTestId('create-order-btn').click();
      await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('exam-mnemonic-input').fill('GLU');
      await page.getByTestId('container-type-input').fill('RedTop');
      await page.getByTestId('request-exam-btn').click();
      await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      await page.goto('/orders');
      await page.getByTestId('order-list-row').first().click();
      await expect(page).toHaveURL(
        /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
        { timeout: 5_000 }
      );

      await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });
      await expect(page.getByTestId('exam-item-row').first()).toBeVisible({ timeout: 5_000 });
    });

    test('LabTechnician is redirected to /unauthorized when accessing /orders/:id', async ({ page }) => {
      await loginAsLabTechnician(page);
      await page.goto('/orders/00000000-0000-0000-0000-000000000001');
      await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
    });

    test('Receptionist can Accept a Requested exam item — status updates to Accepted', async ({ page }) => {
      await loginAsReceptionist(page);

      await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
      await page.getByTestId('create-order-btn').click();
      await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('exam-mnemonic-input').fill('GLU');
      await page.getByTestId('container-type-input').fill('RedTop');
      await page.getByTestId('request-exam-btn').click();
      await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      await page.goto('/orders');
      await page.getByTestId('order-list-row').first().click();
      await expect(page).toHaveURL(
        /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
        { timeout: 5_000 }
      );

      await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });
      await expect(page.getByTestId('accept-btn').first()).toBeVisible({ timeout: 5_000 });

      await page.getByTestId('accept-btn').first().click();

      await expect(page.getByTestId('item-status').first()).toHaveText('Accepted', { timeout: 10_000 });
    });

    test('Receptionist can Reject an exam item with a reason — status updates to Rejected', async ({ page }) => {
      await loginAsReceptionist(page);

      await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
      await page.getByTestId('create-order-btn').click();
      await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('exam-mnemonic-input').fill('HGB');
      await page.getByTestId('container-type-input').fill('EDTA');
      await page.getByTestId('request-exam-btn').click();
      await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      await page.goto('/orders');
      await page.getByTestId('order-list-row').first().click();
      await expect(page).toHaveURL(
        /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
        { timeout: 5_000 }
      );

      await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('reject-btn').first().click();

      await expect(page.getByTestId('reject-reason-form')).toBeVisible({ timeout: 5_000 });
      await page.getByTestId('reject-reason-input').fill('Hemolyzed sample');
      await page.getByTestId('confirm-reject-btn').click();

      await expect(page.getByTestId('item-status').first()).toHaveText('Rejected', { timeout: 10_000 });
    });
  });
  ```

- [ ] **Step 2: Run the new E2E tests to confirm they fail**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn e2e --grep "Order Detail"
  ```

  Expected: tests fail — either the navigation times out (route doesn't exist) or `order-detail` element is not found. The LabTechnician role-guard test will also fail because the route doesn't exist yet.

- [ ] **Step 3: Commit the failing tests**

  ```bash
  git add src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/orders.spec.ts
  git commit -m "test(spa): add failing E2E tests for order detail page (F-12)"
  ```

---

## Task 2: Add route + create skeleton component (F-13)

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/app.routes.ts`
- Create: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.ts`

- [ ] **Step 1: Add `orders/:id` route to `app.routes.ts`**

  In `app.routes.ts`, insert the `orders/:id` route **immediately after** `orders/new` and **before** `orders`. The three order-related routes must appear in this order (most specific first):

  ```typescript
  {
    path: 'orders/new',
    canActivate: [roleGuard('Receptionist', 'ITAdmin')],
    loadComponent: () =>
      import('./features/orders/new-order.component').then(m => m.NewOrderComponent),
  },
  {
    path: 'orders/:id',
    canActivate: [roleGuard('Receptionist', 'Physician', 'ITAdmin')],
    loadComponent: () =>
      import('./features/orders/order-detail.component').then(m => m.OrderDetailComponent),
  },
  {
    path: 'orders',
    canActivate: [roleGuard('Receptionist', 'Physician', 'ITAdmin')],
    loadComponent: () =>
      import('./features/orders/order-list.component').then(m => m.OrderListComponent),
  },
  ```

- [ ] **Step 2: Create the skeleton component**

  Create `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.ts`:

  ```typescript
  import { Component } from '@angular/core';

  @Component({
    selector: 'app-order-detail',
    standalone: true,
    template: `<div data-testid="order-detail"></div>`,
  })
  export class OrderDetailComponent {}
  ```

- [ ] **Step 3: Verify TypeScript compiles cleanly**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn build
  ```

  Expected: `Build succeeded` — no errors.

- [ ] **Step 4: Confirm role-guard E2E test now passes**

  ```bash
  yarn e2e --grep "LabTechnician is redirected to /unauthorized when accessing /orders"
  ```

  Expected: PASS. The route now exists so the guard can redirect LabTech. The content-based tests still fail (empty template).

---

## Task 3: Write failing integration tests for OrderDetailComponent (F-14)

**Files:**
- Create: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.integration.spec.ts`

- [ ] **Step 1: Create `order-detail.component.integration.spec.ts`**

  ```typescript
  import { TestBed, ComponentFixture } from '@angular/core/testing';
  import { signal } from '@angular/core';
  import { ActivatedRoute } from '@angular/router';
  import { OrderDetailComponent } from './order-detail.component';
  import { OrdersService } from './orders.service';
  import type { OrderDetails, ExamItem } from '../../core/domain/order-details';

  describe('OrderDetailComponent (integration)', () => {
    let fixture: ComponentFixture<OrderDetailComponent>;
    let mockService: Partial<OrdersService>;
    let detailsSignal: ReturnType<typeof signal<OrderDetails | null>>;

    const orderId = 'order-abc-123';

    const requestedItem: ExamItem = {
      orderItemId: 'item-1',
      specimenMnemonic: 'BLD',
      materialType: 'Whole Blood',
      containerType: 'EDTA',
      additive: 'K2EDTA',
      processingType: 'Standard',
      storageCondition: 'RT',
      reasonForRejection: null,
      status: 'Requested',
      requestedAt: '2026-05-14T08:00:00Z',
      canceledAt: null,
      onHoldAt: null,
      acceptedAt: null,
      rejectedAt: null,
      inProgressAt: null,
      partiallyCompletedAt: null,
      completedAt: null,
    };

    const acceptedItem: ExamItem = {
      ...requestedItem,
      orderItemId: 'item-2',
      status: 'Accepted',
      acceptedAt: '2026-05-14T09:00:00Z',
    };

    const baseOrder: OrderDetails = {
      orderId,
      patientId: 'patient-1',
      requestedBy: 'Dr. Smith',
      orderPriority: 'Routine',
      requestedAt: '2026-05-14T07:00:00Z',
      items: [requestedItem, acceptedItem],
    };

    beforeEach(async () => {
      detailsSignal = signal<OrderDetails | null>(null);

      mockService = {
        orderDetails: detailsSignal,
        loadOrderDetails: vi.fn().mockResolvedValue(undefined),
        acceptExam: vi.fn().mockResolvedValue(undefined),
        cancelExam: vi.fn().mockResolvedValue(undefined),
        rejectExam: vi.fn().mockResolvedValue(undefined),
        placeExamOnHold: vi.fn().mockResolvedValue(undefined),
        placeExamInProgress: vi.fn().mockResolvedValue(undefined),
        partiallyCompleteExam: vi.fn().mockResolvedValue(undefined),
      };

      await TestBed.configureTestingModule({
        imports: [OrderDetailComponent],
        providers: [
          { provide: OrdersService, useValue: mockService },
          { provide: ActivatedRoute, useValue: { snapshot: { params: { id: orderId } } } },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(OrderDetailComponent);
      fixture.detectChanges();
    });

    afterEach(() => TestBed.resetTestingModule());

    it('calls loadOrderDetails() with the route id on init', () => {
      expect(mockService.loadOrderDetails).toHaveBeenCalledWith(orderId);
    });

    it('renders the order-detail container', () => {
      const el = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="order-detail"]');
      expect(el).not.toBeNull();
    });

    it('renders exam-items-table when order details are loaded', () => {
      detailsSignal.set(baseOrder);
      fixture.detectChanges();

      const table = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="exam-items-table"]');
      expect(table).not.toBeNull();
    });

    it('renders one row per exam item', () => {
      detailsSignal.set(baseOrder);
      fixture.detectChanges();

      const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="exam-item-row"]');
      expect(rows).toHaveLength(2);
    });

    it('shows empty-items cell when items array is empty', () => {
      detailsSignal.set({ ...baseOrder, items: [] });
      fixture.detectChanges();

      const empty = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="empty-items"]');
      expect(empty).not.toBeNull();
    });

    it('shows Accept button for a Requested item', () => {
      detailsSignal.set({ ...baseOrder, items: [requestedItem] });
      fixture.detectChanges();

      const btn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="accept-btn"]');
      expect(btn).not.toBeNull();
    });

    it('does not show Accept button for an Accepted item', () => {
      detailsSignal.set({ ...baseOrder, items: [acceptedItem] });
      fixture.detectChanges();

      const btn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="accept-btn"]');
      expect(btn).toBeNull();
    });

    it('clicking Accept calls ordersService.acceptExam with orderId and itemId', async () => {
      detailsSignal.set({ ...baseOrder, items: [requestedItem] });
      fixture.detectChanges();

      const btn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]');
      btn!.click();
      await fixture.whenStable();

      expect(mockService.acceptExam).toHaveBeenCalledWith(orderId, 'item-1');
    });

    it('clicking Accept reloads order details', async () => {
      detailsSignal.set({ ...baseOrder, items: [requestedItem] });
      fixture.detectChanges();

      (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="accept-btn"]')!.click();
      await fixture.whenStable();

      expect(mockService.loadOrderDetails).toHaveBeenCalledTimes(2);
    });

    it('clicking Reject button shows reject-reason-form inline', () => {
      detailsSignal.set({ ...baseOrder, items: [requestedItem] });
      fixture.detectChanges();

      (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="reject-btn"]')!.click();
      fixture.detectChanges();

      const form = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="reject-reason-form"]');
      expect(form).not.toBeNull();
    });

    it('clicking Confirm Reject calls ordersService.rejectExam with reason', async () => {
      detailsSignal.set({ ...baseOrder, items: [requestedItem] });
      fixture.detectChanges();

      (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="reject-btn"]')!.click();
      fixture.detectChanges();

      const input = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('[data-testid="reject-reason-input"]');
      input!.value = 'Hemolyzed sample';
      input!.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="confirm-reject-btn"]')!.click();
      await fixture.whenStable();

      expect(mockService.rejectExam).toHaveBeenCalledWith(orderId, 'item-1', 'Hemolyzed sample');
    });

    it('shows In Progress button for an Accepted item', () => {
      detailsSignal.set({ ...baseOrder, items: [acceptedItem] });
      fixture.detectChanges();

      const btn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="in-progress-btn"]');
      expect(btn).not.toBeNull();
    });
  });
  ```

- [ ] **Step 2: Run the integration tests to confirm they fail**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn test --reporter=verbose order-detail.component.integration
  ```

  Expected: most tests fail (skeleton component has no real logic or template).

- [ ] **Step 3: Commit the failing tests**

  ```bash
  git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.integration.spec.ts
  git commit -m "test(spa): add failing integration tests for OrderDetailComponent (F-14)"
  ```

---

## Task 4: Implement OrderDetailComponent (F-15)

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.ts`

- [ ] **Step 1: Replace the skeleton with the full implementation**

  Replace the full content of `order-detail.component.ts`:

  ```typescript
  import { Component, OnInit, inject, signal } from '@angular/core';
  import { FormsModule } from '@angular/forms';
  import { RouterLink, ActivatedRoute } from '@angular/router';
  import { OrdersService } from './orders.service';

  @Component({
    selector: 'app-order-detail',
    standalone: true,
    imports: [FormsModule, RouterLink],
    template: `
      <div class="page" data-testid="order-detail">
        <div class="page-header">
          <a class="back-link" routerLink="/orders">← Back to Orders</a>
          <h1 class="page-title">Order Detail</h1>
        </div>

        @if (ordersService.orderDetails(); as details) {
          <div class="order-meta" data-testid="order-meta">
            <p>Patient: <span data-testid="patient-id">{{ details.patientId }}</span></p>
            <p>Requested By: <span data-testid="requested-by">{{ details.requestedBy }}</span></p>
            <p>Priority: <span data-testid="order-priority">{{ details.orderPriority }}</span></p>
            <p>Requested At: <span data-testid="order-requested-at">{{ details.requestedAt }}</span></p>
          </div>

          <h2 class="section-title">Exam Items</h2>

          <table data-testid="exam-items-table" class="items-table">
            <thead>
              <tr>
                <th>Specimen</th>
                <th>Material</th>
                <th>Container</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (item of details.items; track item.orderItemId) {
                <tr data-testid="exam-item-row">
                  <td>{{ item.specimenMnemonic }}</td>
                  <td>{{ item.materialType }}</td>
                  <td>{{ item.containerType }}</td>
                  <td>
                    <span data-testid="item-status" class="status-badge">{{ item.status }}</span>
                  </td>
                  <td class="actions-cell">
                    @if (item.status === 'Requested' || item.status === 'OnHold') {
                      <button data-testid="accept-btn" (click)="onAccept(item.orderItemId)">Accept</button>
                    }
                    @if (item.status === 'Accepted') {
                      <button data-testid="in-progress-btn" (click)="onPlaceInProgress(item.orderItemId)">In Progress</button>
                    }
                    @if (item.status === 'InProgress') {
                      <button data-testid="partially-complete-btn" (click)="onPartiallyComplete(item.orderItemId)">Partially Complete</button>
                    }
                    @if (item.status === 'Requested' || item.status === 'Accepted') {
                      <button data-testid="reject-btn" (click)="startReject(item.orderItemId)">Reject</button>
                    }
                    @if (item.status === 'Accepted') {
                      <button data-testid="on-hold-btn" (click)="startOnHold(item.orderItemId)">On Hold</button>
                    }
                    @if (item.status !== 'Canceled' && item.status !== 'Rejected' && item.status !== 'Completed' && item.status !== 'PartiallyCompleted') {
                      <button data-testid="cancel-btn" (click)="onCancel(item.orderItemId)">Cancel</button>
                    }
                    @if (activeRejectItemId() === item.orderItemId) {
                      <div data-testid="reject-reason-form" class="reason-form">
                        <input
                          data-testid="reject-reason-input"
                          type="text"
                          [(ngModel)]="rejectReason"
                          placeholder="Reason for rejection"
                        />
                        <button
                          data-testid="confirm-reject-btn"
                          (click)="onReject(item.orderItemId)"
                          [disabled]="!rejectReason.trim()"
                        >Confirm Reject</button>
                        <button data-testid="cancel-reject-btn" (click)="activeRejectItemId.set(null)">Cancel</button>
                      </div>
                    }
                    @if (activeOnHoldItemId() === item.orderItemId) {
                      <div data-testid="on-hold-reason-form" class="reason-form">
                        <input
                          data-testid="on-hold-reason-input"
                          type="text"
                          [(ngModel)]="onHoldReason"
                          placeholder="Reason for hold"
                        />
                        <button
                          data-testid="confirm-on-hold-btn"
                          (click)="onPlaceOnHold(item.orderItemId)"
                          [disabled]="!onHoldReason.trim()"
                        >Confirm On Hold</button>
                        <button data-testid="cancel-on-hold-btn" (click)="activeOnHoldItemId.set(null)">Cancel</button>
                      </div>
                    }
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="5" data-testid="empty-items" class="empty-cell">No exam items found.</td>
                </tr>
              }
            </tbody>
          </table>
        }
      </div>
    `,
    styles: [`
      .page { max-width: 1000px; margin: 2rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
      .page-header { display: flex; align-items: baseline; gap: 1.5rem; margin-bottom: 1.5rem; }
      .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin: 0; }
      .back-link { font-size: 0.875rem; color: #2c7be5; text-decoration: none; }
      .back-link:hover { text-decoration: underline; }
      .order-meta { background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 6px; padding: 1rem 1.25rem; margin-bottom: 1.5rem; }
      .order-meta p { margin: 0.25rem 0; font-size: 0.875rem; color: #374151; }
      .section-title { font-size: 1.1rem; font-weight: 600; margin-bottom: 0.75rem; }
      .items-table { width: 100%; border-collapse: collapse; }
      .items-table th { text-align: left; padding: 0.5rem 0.75rem; border-bottom: 2px solid #e5e7eb; font-size: 0.75rem; text-transform: uppercase; color: #6b7280; }
      .items-table td { padding: 0.75rem; border-bottom: 1px solid #f3f4f6; font-size: 0.875rem; vertical-align: top; }
      .status-badge { display: inline-block; padding: 0.2rem 0.6rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 500; background: #e5e7eb; color: #374151; }
      .actions-cell { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: flex-start; }
      .actions-cell button { padding: 0.3rem 0.7rem; font-size: 0.8rem; border: 1px solid #d1d5db; border-radius: 4px; background: #fff; cursor: pointer; }
      .actions-cell button:hover { background: #f3f4f6; }
      .reason-form { display: flex; flex-direction: column; gap: 0.4rem; margin-top: 0.5rem; min-width: 220px; }
      .reason-form input { padding: 0.35rem 0.6rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.8rem; }
      .empty-cell { text-align: center; color: #9ca3af; padding: 2rem; }
    `],
  })
  export class OrderDetailComponent implements OnInit {
    protected readonly ordersService = inject(OrdersService);
    private readonly route = inject(ActivatedRoute);

    protected readonly activeRejectItemId = signal<string | null>(null);
    protected readonly activeOnHoldItemId = signal<string | null>(null);
    protected rejectReason = '';
    protected onHoldReason = '';

    ngOnInit(): void {
      void this.ordersService.loadOrderDetails(this.route.snapshot.params['id'] as string);
    }

    protected async onAccept(itemId: string): Promise<void> {
      const orderId = this.route.snapshot.params['id'] as string;
      await this.ordersService.acceptExam(orderId, itemId);
      void this.ordersService.loadOrderDetails(orderId);
    }

    protected startReject(itemId: string): void {
      this.activeRejectItemId.set(itemId);
      this.rejectReason = '';
    }

    protected async onReject(itemId: string): Promise<void> {
      const orderId = this.route.snapshot.params['id'] as string;
      await this.ordersService.rejectExam(orderId, itemId, this.rejectReason.trim());
      this.activeRejectItemId.set(null);
      this.rejectReason = '';
      void this.ordersService.loadOrderDetails(orderId);
    }

    protected async onCancel(itemId: string): Promise<void> {
      const orderId = this.route.snapshot.params['id'] as string;
      await this.ordersService.cancelExam(orderId, itemId);
      void this.ordersService.loadOrderDetails(orderId);
    }

    protected startOnHold(itemId: string): void {
      this.activeOnHoldItemId.set(itemId);
      this.onHoldReason = '';
    }

    protected async onPlaceOnHold(itemId: string): Promise<void> {
      const orderId = this.route.snapshot.params['id'] as string;
      await this.ordersService.placeExamOnHold(orderId, itemId, this.onHoldReason.trim());
      this.activeOnHoldItemId.set(null);
      this.onHoldReason = '';
      void this.ordersService.loadOrderDetails(orderId);
    }

    protected async onPlaceInProgress(itemId: string): Promise<void> {
      const orderId = this.route.snapshot.params['id'] as string;
      await this.ordersService.placeExamInProgress(orderId, itemId);
      void this.ordersService.loadOrderDetails(orderId);
    }

    protected async onPartiallyComplete(itemId: string): Promise<void> {
      const orderId = this.route.snapshot.params['id'] as string;
      await this.ordersService.partiallyCompleteExam(orderId, itemId);
      void this.ordersService.loadOrderDetails(orderId);
    }
  }
  ```

- [ ] **Step 2: TypeScript compile check**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn build
  ```

  Expected: `Build succeeded` — no type errors.

- [ ] **Step 3: Run integration tests**

  ```bash
  yarn test --reporter=verbose order-detail.component.integration
  ```

  Expected: all 12 integration tests pass.

- [ ] **Step 4: Run the full unit test suite to check for regressions**

  ```bash
  yarn test
  ```

  Expected: all tests pass (previous suite + 12 new tests).

- [ ] **Step 5: Commit route + implementation together**

  ```bash
  git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/order-detail.component.ts
  git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/app.routes.ts
  git commit -m "feat(spa): implement order detail page with route and exam lifecycle actions (F-13, F-15)"
  ```

---

## Task 5: Final Verification (F-16)

- [ ] **Step 1: Run full unit + integration test suite**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn test
  ```

  Expected: all tests pass — no regressions in existing suites.

- [ ] **Step 2: Run full E2E suite**

  Requires: Angular dev server (`ng serve`), API running (`dotnet run --project src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj`), PostgreSQL up (`docker-compose -f development-compose.yaml up -d`).

  ```bash
  yarn e2e
  ```

  Expected: all tests pass including the 5 new Order Detail tests.

- [ ] **Step 3: Mark phase complete**

  ```bash
  git add docs/superpowers/plans/2026-05-14-phase6-order-detail.md
  git commit -m "docs(plans): mark phase 6 tasks complete"
  ```

---

## Verification Summary

| Check | Command | Expected |
|-------|---------|----------|
| TypeScript compiles | `yarn build` in `hc-lis-spa` | No errors |
| Unit + integration | `yarn test` in `hc-lis-spa` | All pass |
| E2E: detail container | `yarn e2e --grep "sees order detail page"` | PASS |
| E2E: exam items table | `yarn e2e --grep "shows exam items table"` | PASS |
| E2E: role guard | `yarn e2e --grep "LabTechnician is redirected.*orders"` | PASS |
| E2E: Accept action | `yarn e2e --grep "can Accept a Requested"` | PASS |
| E2E: Reject with reason | `yarn e2e --grep "can Reject"` | PASS |
| Full E2E suite | `yarn e2e` | All pass, no regressions |
