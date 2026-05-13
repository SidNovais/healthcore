# Phase 4 — Frontend: SDK + Infrastructure Layer

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the Angular infrastructure layer for the TestOrders feature — domain types, service interfaces, SDK adapter, and unit tests — with the `@hc-lis/api-client` SDK regenerated from the updated backend and the `getOrderDetails` items mapping fully implemented.

**Architecture:** Most of Phase 4 (F-2 through F-7) is already written in staged/untracked files. What remains: (1) a backend prerequisite — `OrderDetailsDto` currently has no `Items` field, so `GET /orders/{orderId}` doesn't return exam items; fix this before regenerating the SDK, or Phase 6's Order Detail page will have no items to display. (2) Regenerate and rebuild the `@hc-lis/api-client` SDK with the API running. (3) Fix the `items: []` placeholder in `sdk-orders-api.ts`.

**Tech Stack:** .NET 10 / C# 13 / Dapper for backend; Angular 21 / TypeScript 5.9 / `@hey-api/openapi-ts` for SDK generation; Vitest for unit tests; Playwright for E2E.

**E2E note:** Phase 4 is infrastructure only — no Angular components are built here, so no Playwright tests are written in this phase. E2E tests (F-8) are the FIRST step of Phase 5 and must be written before `OrderListComponent` is implemented.

---

## Files

### Backend (prerequisite fix)
- Modify: `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/OrderDetailsDto.cs`
- Modify: `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/GetOrderDetailsQueryHandler.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/GetOrderDetailsWithItemsTests.cs`

### Frontend (already written — need SDK fix + items mapping)
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/infrastructure/orders/sdk-orders-api.ts`
- Generated (rebuild): `src/HC.LIS.Frontend/packages/hc-lis-api-client/src/generated/`

### Already written (commit-only in Task 4)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/domain/order-list-item.ts` (new)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/domain/order-details.ts` (new)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/application/i-orders-port.ts` (modified)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/infrastructure/orders/i-orders-api.ts` (modified)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/infrastructure/orders/sdk-orders-adapter.ts` (modified)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/orders.service.ts` (modified)
- `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/orders/orders.service.spec.ts` (modified)

---

## Task 1: Backend — Add items to GetOrderDetailsQuery (TDD)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/GetOrderDetailsWithItemsTests.cs`
- Modify: `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/OrderDetailsDto.cs`
- Modify: `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/GetOrderDetailsQueryHandler.cs`

- [ ] **Step 1: Write the failing integration test**

  Create `src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/GetOrderDetailsWithItemsTests.cs`:

  ```csharp
  using System.Linq;
  using FluentAssertions;
  using HC.Core.Domain;
  using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;
  using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

  namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

  public class GetOrderDetailsWithItemsTests : TestBase
  {
      public GetOrderDetailsWithItemsTests() : base(Guid.CreateVersion7())
      {
          OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
          GetEventually(
              new GetOrderDetailFromTestOrdersProbe(OrderSampleData.OrderId, TestOrdersModule),
              15000
          ).GetAwaiter().GetResult();
          TestOrdersModule.ExecuteCommandAsync(
              new RequestExamCommand(
                  OrderSampleData.OrderId,
                  OrderSampleData.OrderItemId,
                  OrderSampleData.ExamMnemonic,
                  OrderSampleData.SpecimenMnemonic,
                  OrderSampleData.MaterialType,
                  OrderSampleData.ContainerType,
                  OrderSampleData.Additive,
                  OrderSampleData.ProcessingType,
                  OrderSampleData.StorageCondition,
                  SystemClock.Now
              )
          ).GetAwaiter().GetResult();
          GetEventually(
              new GetOrderItemDetailFromTestOrdersProbe(OrderSampleData.OrderItemId, TestOrdersModule),
              15000
          ).GetAwaiter().GetResult();
      }

      [Fact]
      public async void GetOrderDetailsIncludesItems()
      {
          OrderDetailsDto? result = await TestOrdersModule
              .ExecuteQueryAsync(new GetOrderDetailsQuery(OrderSampleData.OrderId))
              .ConfigureAwait(true);

          result.Should().NotBeNull();
          result!.OrderId.Should().Be(OrderSampleData.OrderId);
          result.Items.Should().HaveCount(1);

          var item = result.Items.First();
          item.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
          item.SpecimenMnemonic.Should().Be(OrderSampleData.SpecimenMnemonic);
          item.Status.Should().Be("Requested");
      }
  }
  ```

- [ ] **Step 2: Run test to verify it fails (compile error expected)**

  ```bash
  dotnet build src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
  ```

  Expected: build error — `'OrderDetailsDto' does not contain a definition for 'Items'`

- [ ] **Step 3: Commit the failing test**

  ```bash
  git add src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/Orders/GetOrderDetailsWithItemsTests.cs
  git commit -m "test(test-orders): add GetOrderDetailsWithItems integration test"
  ```

- [ ] **Step 4: Update OrderDetailsDto to add Items property**

  Replace the full content of `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/OrderDetailsDto.cs`:

  ```csharp
  using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

  namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

  public class OrderDetailsDto
  {
      public Guid OrderId { get; set; }
      public Guid PatientId { get; set; }
      public Guid RequestedBy { get; set; }
      public string OrderPriority { get; set; } = string.Empty;
      public DateTime RequestedAt { get; set; }
      public IReadOnlyCollection<OrderItemDetailsDto> Items { get; set; } = [];
  }
  ```

- [ ] **Step 5: Update GetOrderDetailsQueryHandler to fetch items**

  Replace the full content of `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/GetOrderDetailsQueryHandler.cs`:

  ```csharp
  using System.Data;
  using Dapper;
  using HC.Core.Infrastructure.Data;
  using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;
  using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

  namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

  internal class GetOrderDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
  ) : IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto?>
  {
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<OrderDetailsDto?> Handle(
      GetOrderDetailsQuery query,
      CancellationToken cancellationToken
    )
    {
      const string sql = @"
        SELECT
          od.""Id"" AS ""OrderId"",
          od.""PatientId"",
          od.""RequestedBy"",
          od.""Priority"" AS ""OrderPriority"",
          od.""RequestedAt""
        FROM test_orders.""OrderDetails"" od
        WHERE od.""Id"" = @OrderId;

        SELECT
          oid.""Id"" AS ""OrderItemId"",
          oid.""OrderId"",
          oid.""SpecimenMnemonic"",
          oid.""MaterialType"",
          oid.""ContainerType"",
          oid.""Additive"",
          oid.""ProcessingType"",
          oid.""StorageCondition"",
          oid.""Status"",
          COALESCE(oid.""ReasonForRejection"", '') AS ""ReasonForRejection"",
          oid.""RequestedAt"",
          oid.""CanceledAt"",
          oid.""OnHoldAt"",
          oid.""AcceptedAt"",
          oid.""RejectedAt"",
          oid.""InProgressAt"",
          oid.""PartiallyCompletedAt"",
          oid.""CompletedAt""
        FROM test_orders.""OrderItemDetails"" oid
        WHERE oid.""OrderId"" = @OrderId";

      IDbConnection connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to get order details");
      using SqlMapper.GridReader multi = await connection
        .QueryMultipleAsync(sql, new { query.OrderId })
        .ConfigureAwait(false);
      OrderDetailsDto? order = await multi
        .ReadFirstOrDefaultAsync<OrderDetailsDto>()
        .ConfigureAwait(false);
      if (order is null) return null;
      order.Items = (await multi.ReadAsync<OrderItemDetailsDto>().ConfigureAwait(false))
        .ToList()
        .AsReadOnly();
      return order;
    }
  }
  ```

- [ ] **Step 6: Run the integration test to verify it passes**

  ```bash
  dotnet test --filter "FullyQualifiedName~GetOrderDetailsIncludesItems" src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
  ```

  Expected: `Passed  GetOrderDetailsWithItemsTests.GetOrderDetailsIncludesItems`

- [ ] **Step 7: Run all integration tests to check for regressions**

  ```bash
  dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj
  ```

  Expected: all tests pass.

- [ ] **Step 8: Build the full API to confirm no compile errors**

  ```bash
  dotnet build src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj
  ```

  Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 9: Commit the backend fix**

  ```bash
  git add src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/OrderDetailsDto.cs
  git add src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/GetOrderDetails/GetOrderDetailsQueryHandler.cs
  git commit -m "feat(test-orders): include exam items in GetOrderDetails response"
  ```

---

## Task 2: Regenerate SDK (F-1)

**Files:**
- Generated: `src/HC.LIS.Frontend/packages/hc-lis-api-client/src/generated/` (fully regenerated)

- [ ] **Step 1: Start the API in the background**

  In a separate terminal:
  ```bash
  dotnet run --project src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj
  ```

  Wait until you see `Now listening on: http://localhost:5000` in the output before proceeding.

- [ ] **Step 2: Generate the SDK from the running API**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-api-client
  yarn generate
  ```

  Expected: no errors; `src/generated/` is updated with new functions including `getOrderList`, `getOrderDetails`, `acceptExam`, `cancelExam`, `rejectExam`, `placeExamOnHold`, `placeExamInProgress`, `partiallyCompleteExam`.

  Verify the new functions exist in the generated output:
  ```bash
  grep -l "getOrderList\|placeExamInProgress" src/generated/
  ```

- [ ] **Step 3: Build the SDK package**

  ```bash
  yarn build
  ```

  Expected: `dist/index.js` and `dist/index.cjs` updated, no TypeScript errors.

- [ ] **Step 4: Reinstall in the SPA to pick up the new build**

  ```bash
  cd ../hc-lis-spa
  yarn install
  ```

  Expected: symlink in `node_modules/@hc-lis/api-client` points to the rebuilt package.

- [ ] **Step 5: Stop the API** (Ctrl+C in its terminal)

---

## Task 3: Fix getOrderDetails items mapping

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/infrastructure/orders/sdk-orders-api.ts`

- [ ] **Step 1: Check the generated SDK type for the order details DTO**

  Open `src/HC.LIS.Frontend/packages/hc-lis-api-client/src/generated/types.gen.ts` and find the type returned by `getOrderDetails`. Look for a type with an `items` array property. Note its exact TypeScript type name — you'll use it in the import below if needed.

- [ ] **Step 2: Update the import in sdk-orders-api.ts**

  In `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/infrastructure/orders/sdk-orders-api.ts`, update the import for domain types:

  ```typescript
  import type { OrderDetails, ExamItemStatus } from '../../domain/order-details';
  ```

- [ ] **Step 3: Replace the getOrderDetails method with items mapping**

  Replace the existing `getOrderDetails` method body in `sdk-orders-api.ts`:

  ```typescript
  async getOrderDetails(orderId: string): Promise<OrderDetails> {
    const result = await sdkGetOrderDetails({ path: { orderId } });
    const dto = result.data;
    return {
      orderId: dto?.orderId ?? '',
      patientId: dto?.patientId ?? '',
      requestedBy: dto?.requestedBy ?? '',
      orderPriority: dto?.orderPriority ?? '',
      requestedAt: dto?.requestedAt ?? '',
      items: (dto?.items ?? []).map(item => ({
        orderItemId: item.orderItemId ?? '',
        specimenMnemonic: item.specimenMnemonic ?? '',
        materialType: item.materialType ?? '',
        containerType: item.containerType ?? '',
        additive: item.additive ?? '',
        processingType: item.processingType ?? '',
        storageCondition: item.storageCondition ?? '',
        reasonForRejection: item.reasonForRejection ?? null,
        status: (item.status ?? 'Requested') as ExamItemStatus,
        requestedAt: item.requestedAt ?? '',
        canceledAt: item.canceledAt ?? null,
        onHoldAt: item.onHoldAt ?? null,
        acceptedAt: item.acceptedAt ?? null,
        rejectedAt: item.rejectedAt ?? null,
        inProgressAt: item.inProgressAt ?? null,
        partiallyCompletedAt: item.partiallyCompletedAt ?? null,
        completedAt: item.completedAt ?? null,
      })),
    };
  }
  ```

  > **Note:** If TypeScript reports that `item.someField` doesn't exist, the SDK generated a different property name. Check `types.gen.ts` for the exact field names on the item DTO type and update accordingly.

- [ ] **Step 4: TypeScript compile check**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn build
  ```

  Expected: `Build at: ... - Time: ...ms` with no type errors.

---

## Task 4: Verify unit tests pass

**Files:** no changes — verification only.

- [ ] **Step 1: Run the SPA unit tests**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  yarn test
  ```

  Expected: all tests pass including the 14 `OrdersService` cases in `orders.service.spec.ts`:
  - `order signal starts as null`
  - `createOrder() calls port.createOrder ...`
  - `createOrder() sets order signal on success`
  - `createOrder() propagates error without setting signal`
  - `requestExam() calls port.requestExam ...`
  - `requestExam() does not change order signal`
  - `orderList signal starts as empty array`
  - `loadOrderList() calls port.getOrderList and sets orderList signal`
  - `orderDetails signal starts as null`
  - `loadOrderDetails() calls port.getOrderDetails with orderId and sets orderDetails signal`
  - `acceptExam() calls port.acceptExam with orderId and itemId`
  - `cancelExam() calls port.cancelExam with orderId and itemId`
  - `rejectExam() calls port.rejectExam with orderId, itemId, and reason`
  - `placeExamOnHold() calls port.placeExamOnHold with orderId, itemId, and reason`
  - `placeExamInProgress() calls port.placeExamInProgress with orderId and itemId`
  - `partiallyCompleteExam() calls port.partiallyCompleteExam with orderId and itemId`

---

## Task 5: Commit Phase 4 frontend changes

**Files:** all staged/untracked frontend files from F-2 through F-7 plus the items mapping fix.

- [ ] **Step 1: Commit F-1 (SDK regeneration)**

  > **Note:** `src/generated/` is listed in `.gitignore` for the api-client package — `git add src/generated/` will produce no changes. Skip this commit step; the SDK is regenerated locally as part of the dev workflow.

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-api-client
  git add src/generated/
  git commit -m "feat(spa): regenerate api-client SDK with order list, details, and exam lifecycle endpoints"
  ```

- [x] **Step 2: Commit F-2 through F-5 (domain types + interfaces)**

  ```bash
  cd src/HC.LIS.Frontend/packages/hc-lis-spa
  git add src/app/core/domain/order-list-item.ts
  git add src/app/core/domain/order-details.ts
  git add src/app/core/application/i-orders-port.ts
  git add src/app/core/infrastructure/orders/i-orders-api.ts
  git add src/app/core/infrastructure/orders/sdk-orders-adapter.ts
  git add src/app/core/infrastructure/orders/sdk-orders-api.ts
  git commit -m "feat(spa): add order domain types and extend IOrdersApi/IOrdersPort with lifecycle methods"
  ```

- [x] **Step 3: Commit F-6 and F-7 (service + unit tests)**

  ```bash
  git add src/app/features/orders/orders.service.spec.ts
  git commit -m "test(spa): add OrdersService unit tests for all lifecycle methods"

  git add src/app/features/orders/orders.service.ts
  git commit -m "feat(spa): extend OrdersService with order list, detail, and exam lifecycle methods"
  ```

---

## Verification

1. **Backend integration tests:** `dotnet test` on the IntegrationTests project — all pass including `GetOrderDetailsIncludesItems`
2. **API builds cleanly:** `dotnet build src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj` — 0 errors
3. **SPA unit tests:** `yarn test` in `hc-lis-spa` — all 16+ tests pass
4. **SPA builds:** `yarn build` in `hc-lis-spa` — no TypeScript errors
5. **E2E tests:** Phase 5 begins by writing Playwright tests (F-8) **before** creating `OrderListComponent` — these tests serve as the end-to-end proof that Phase 4's infrastructure is wired correctly

---

## Next Step (Phase 5 reminder)

Phase 5 starts with F-8 — writing the failing E2E test in `e2e/orders.spec.ts`:
- Receptionist sees order list at `/orders` (`data-testid="order-list-table"`)
- Clicking a row navigates to `/orders/<id>`
- LabTechnician is redirected to `/unauthorized`

Write and commit the E2E tests **before** adding the `/orders` route or creating `OrderListComponent`.
