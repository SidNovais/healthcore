# TestOrders Full Lifecycle — Implementation Tasks

> **RULE: Mark every task `[x]` immediately when it is done. Do not batch. Do not wait until the end of the session.**

## Context

The TestOrders module has a complete domain model and Application layer (9 commands, 2 queries)
but the API and SPA are incomplete. This document tracks every task to deliver the full
order lifecycle UX for receptionists and physicians.

**Already done — no work needed:**
- All 9 Application commands (CreateOrder, RequestExam, AcceptExam, CancelExam, RejectExam,
  PlaceExamOnHold, PartiallyCompleteExam, CompleteExam + both internal commands)
- API endpoints: `POST /orders`, `GET /orders/{id}`, `POST /orders/{id}/exams`,
  `GET /orders/exams/{itemId}` (buggy route — see B-1), `POST /orders/{id}/exams/{itemId}/accept`,
  `/cancel`, `/partially-complete`, `/place-on-hold`, `/reject`

---

## Phase 1 — Backend: Fix existing route bug

- [x] **B-1** Fix `GetOrderItemDetails` route: change `GET /orders/exams/{itemId}` →
  `GET /orders/{orderId}/exams/{itemId}` in `OrdersEndpoints.cs` + add `orderId` param to
  `GetOrderItemDetailsEndpoint.Handle()`

---

## Phase 2 — Backend: Missing Application query

- [x] **B-2** Write failing integration test for `GetOrdersListQuery`
  (`Tests/IntegrationTests/Orders/GetOrdersListTests.cs`)
- [x] **B-3** Implement `GetOrdersListQuery` + `GetOrdersListQueryHandler` + `OrderListItemDto`
  under `Application/Orders/GetOrdersList/`
  — use `ISqlConnectionFactory` + Dapper; GROUP BY order, count items
  — fields: `OrderId`, `PatientId`, `RequestedBy`, `OrderPriority`, `RequestedAt`, `ItemCount`

---

## Phase 3 — Backend: Missing API endpoints

- [x] **B-4** Add `GET /api/v1/orders` endpoint (`GetOrderList/GetOrderListEndpoint.cs`)
  — 200 OK with `IReadOnlyCollection<OrderListItemDto>`; wire into `OrdersEndpoints.cs`
- [x] **B-5** Add `POST /api/v1/orders/{orderId}/exams/{itemId}/place-in-progress` endpoint
  (`PlaceExamInProgress/PlaceExamInProgressEndpoint.cs`)
  — no request body; 204 No Content; wire into `OrdersEndpoints.cs`
  — `PlaceExamInProgressCommand` already exists in Application
- [x] **B-6** Build + test verification
  ```bash
  dotnet build src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj
  dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
  ```

---

## Phase 4 — Frontend: SDK + Infrastructure layer

- [x] **F-1** Regenerate `@hc-lis/api-client` SDK after all API changes are compiled
- [x] **F-2** Create domain types:
  - `src/app/core/domain/order-list-item.ts` — `OrderListItem` interface
  - `src/app/core/domain/order-details.ts` — `OrderDetails`, `ExamItem`, `ExamItemStatus` union type
- [x] **F-3** Extend `IOrdersApi` (`core/infrastructure/orders/i-orders-api.ts`) with new methods:
  `getOrderList()`, `getOrderDetails(orderId)`,
  `acceptExam(orderId, itemId)`, `cancelExam(orderId, itemId)`,
  `rejectExam(orderId, itemId, reason)`, `placeExamOnHold(orderId, itemId, reason)`,
  `placeExamInProgress(orderId, itemId)`, `partiallyCompleteExam(orderId, itemId)`
- [x] **F-4** Implement all new methods in `SdkOrdersApi`
- [x] **F-5** Extend `IOrdersPort` (`core/application/i-orders-port.ts`) with the same methods
- [x] **F-6** Extend `OrdersService` (`features/orders/orders.service.ts`) with all new methods
- [x] **F-7** Add unit test cases to `orders.service.spec.ts` for each new method

---

## Phase 5 — Frontend: Order List page

- [ ] **F-8** [E2E first] Add tests to `e2e/orders.spec.ts`:
  - Receptionist sees order list at `/orders` (`data-testid="order-list-table"`)
  - Clicking a row navigates to `/orders/<id>`
  - LabTechnician is redirected to `/unauthorized`
- [ ] **F-9** Add `/orders` route to `app.routes.ts`
  — guard: `roleGuard('Receptionist', 'Physician', 'ITAdmin')`
- [ ] **F-10** Implement `OrderListComponent` (`features/orders/order-list.component.ts`)
  — signals-based; table with `data-testid="order-list-row"` per item; each row is a `[routerLink]`

---

## Phase 6 — Frontend: Order Detail page

- [ ] **F-11** [E2E first] Add tests to `e2e/orders.spec.ts`:
  - Receptionist sees `data-testid="order-detail-header"` at `/orders/:id`
  - Exam items listed with `data-testid="exam-item-row"` and `data-testid="exam-status-badge"`
  - Physician can access the page (no redirect)
- [ ] **F-12** Add `/orders/:id` route to `app.routes.ts`
  — guard: `roleGuard('Receptionist', 'Physician', 'ITAdmin')`
- [ ] **F-13** Implement `OrderDetailComponent` (`features/orders/order-detail.component.ts`)
  — reads `id` from `ActivatedRoute`; calls `ordersService.getOrderDetails(id)` on init
  — shows order header + exam item list; `data-testid="back-to-orders-link"` back nav

---

## Phase 7 — Frontend: Exam lifecycle action buttons

Actions live inline within each `exam-item-row`. Only show buttons valid for the current status.

| Status | Valid HTTP-triggered actions |
|---|---|
| Requested | Accept, PlaceOnHold, Cancel |
| OnHold | Accept, Cancel |
| Accepted | PlaceInProgress, Reject, Cancel |
| InProgress | PartiallyComplete, Reject |
| PartiallyCompleted | — (Complete is triggered internally by LabAnalysis) |
| Completed / Rejected / Canceled | — (terminal) |

> `CompleteExam` is enqueued by `WorklistItemCompletedIntegrationEventNotificationHandler` — **no HTTP button**.

- [ ] **F-14** [E2E first] Add full-workflow E2E test to `e2e/orders.spec.ts`:
  1. Create order + request exam → navigate to `/orders/:id`
  2. Click `data-testid="accept-exam-btn"` → assert badge changes to "Accepted"
  3. Click `data-testid="place-in-progress-btn"` → assert badge changes to "InProgress"
  4. Individual tests: reject with reason, cancel, place-on-hold with reason
- [ ] **F-15** Add no-reason action buttons to `OrderDetailComponent`:
  - `data-testid="accept-exam-btn"` → `ordersService.acceptExam(orderId, itemId)`
  - `data-testid="place-in-progress-btn"` → `ordersService.placeExamInProgress(orderId, itemId)`
  - `data-testid="cancel-exam-btn"` → `ordersService.cancelExam(orderId, itemId)`
  - `data-testid="partially-complete-exam-btn"` → `ordersService.partiallyCompleteExam(orderId, itemId)`
- [ ] **F-16** Add reason-required action inline forms to `OrderDetailComponent`:
  - Reject: `data-testid="reject-exam-btn"` reveals `data-testid="reject-reason-input"` +
    `data-testid="confirm-reject-btn"` → `ordersService.rejectExam(orderId, itemId, reason)`
  - PlaceOnHold: `data-testid="place-on-hold-btn"` reveals `data-testid="hold-reason-input"` +
    `data-testid="confirm-hold-btn"` → `ordersService.placeExamOnHold(orderId, itemId, reason)`
  - On success: reload order details to refresh all status badges
  - On 409: show `data-testid="exam-action-error"` with the error message

---

## Phase 8 — Shell navigation

- [ ] **F-17** Add Orders nav link to `ShellComponent` (`core/shell/shell.component.ts`):
  - `data-testid="nav-orders-link"` visible to Receptionist, Physician, ITAdmin roles
  - `[routerLink]="['/orders']"`

---

## Phase 9 — Final verification

- [ ] **V-1** `dotnet test` — all three TestOrders test projects (Unit, Integration, Arch) pass
- [ ] **V-2** `yarn e2e` — all E2E specs pass
