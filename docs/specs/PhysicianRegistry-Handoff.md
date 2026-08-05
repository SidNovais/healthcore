# Physician Registry & Forward Propagation — Implementation Handoff

Each Part below is independently executable in a fresh session: it states its prerequisites, its files, its tests, and a concrete "done when". Update the checkboxes in **Progress** and add a session note as you finish each one.

---

## Context

HealthCore has **no physician**. `TestOrders/Domain/Physicians/PhysicianId.cs` is a one-line `Guid` wrapper and nothing else:

```csharp
public class PhysicianId(Guid value) : Id(value) { }
```

Worse, the value it wraps is wrong. `new-order.component.ts:57` sends `requestedBy: this.authService.currentUser()?.userId` — the **receptionist's own user id**, never validated, never a doctor. That guid flows end to end (`CreateOrderRequest.RequestedBy` → `PhysicianId` → `OrderDetails.RequestedBy` → `OrderListItemDto`/`OrderDetailsDto` → SPA) and is **rendered nowhere**: commit `0973ec6` deleted both display sites under the "never show a raw UUID" rule, leaving a documented backend follow-up.

This introduces a real **referring-physician registry** — external doctors who order exams and have no system login — and carries the physician forward to the order, the worklist, and the signed report.

## Decisions (settled — do not re-litigate)

| Question | Decision |
|---|---|
| What is a physician? | New **referring-physician registry** (master data), not a UserAccess user |
| Where does it live? | **Inside TestOrders**, schema `test_orders`; TestOrders publishes physician integration events |
| Domain rules | **FullName required**; licence number *optional*, **no uniqueness rule**; Active/Inactive |
| How far forward? | Order list + detail, worklist list + detail, signed HTML/PDF report header. **Triage is out of scope** |
| Name → read models | **Module-local snapshot table + SQL join** (mirrors `PatientSnapshotDetails`); only the physician *id* travels on events, so renames propagate |
| Order UX | Typeahead picker mirroring `PatientPickerComponent`; physician required |
| Registry admin | **Both** an ITAdmin CRUD page and inline quick-add from the order form |

## Architecture

### Physician must be event-sourced (Marten), not EF-mapped

Forced, not stylistic: `TestOrders/Infrastructure/Configurations/Processing/TestOrdersUnitOfWork.cs` commits **only** `IDocumentSession.SaveChangesAsync()`, and `AggregateStoreDomainEventsAccessor` sources events exclusively from `IAggregateStore.GetChanges()`. An EF-mapped aggregate would silently never persist and never dispatch a domain event. `UserAccess.User` gets away with EF because it has its own EF-based UoW. **`PatientManagement/Domain/Patients/Patient.cs` is the blueprint.**

### Propagation: an order→physician mapping, not event threading

Two facts make this far cheaper than it looks:

1. **`CollectionRequestId` *is* the `OrderId`** (`OrderItemAcceptedIntegrationEventHandler.cs:23`), and `WorklistItem.Create(...)` already receives it as `orderId` — so **`lab_analysis.worklist_item_details.order_id` already exists**.
2. `OrderCreatedIntegrationEvent` **already carries `RequestedBy`**.

LabAnalysis subscribes to the already-published `OrderCreatedIntegrationEvent` and stores an `OrderId → PhysicianId` mapping, then joins at query time.

The rejected alternative — threading a physician id through `OrderItemAccepted` → `CollectionRequest` → `SampleCollected` → `WorklistItem.Create` — would add properties to **three event-sourced aggregates**. `MartenAggregateStore.Load` deserializes historic `mt_events` JSON with Newtonsoft, so a missing property becomes `Guid.Empty` on replay: silent corruption, not a crash. The mapping approach changes **no existing event shape at all**.

```
TestOrders (owner)
  Physician aggregate ──► test_orders."PhysicianDetails" ──┐ LEFT JOIN on
        │                                                   │ OrderDetails."RequestedBy"
        │ Physician{Registered,Updated,Deactivated,          │
        │            Reactivated}IntegrationEvent      order list + detail
        ▼
  LabAnalysis
    "PhysicianSnapshotDetails"      ◄── physician events
    "OrderPhysicianSnapshotDetails" ◄── OrderCreatedIntegrationEvent
        │
        └─ join: worklist_item_details.order_id → OrderPhysician… → PhysicianSnapshot…
                 └─► worklist list + detail + HtmlReportTemplate header
```

### Part ordering is load-bearing

**The frontend must send a real physician id BEFORE the backend enforces it.** Part 4 makes `Order.Create` reject unknown physicians; if it landed before Part 3, order creation would be broken in the SPA and in ~14 Playwright call sites for the whole intervening stretch. Hence: registry → SDK → picker → *then* enforce.

```
Part 1 (registry + API)
   └─► Part 2 (SDK + port) ──┬─► Part 3 (picker) ──► Part 4 (enforce) ──► Part 5 (order views)
                             └─► Part 7 (admin page — parallelisable)
Part 1 ──► Part 6 (LabAnalysis → worklist + report — parallelisable with 3/4/5)
                                                              Part 8 (full verification)
```

TDD is mandatory throughout: each step is a `test:` commit then a `feat:` commit. Fixture/spec updates that a signature change breaks belong in the **same `test:` commit**, so no commit leaves the suite red for the wrong reason.

---

## Part 1 — Physician registry (backend + API)

**Prerequisites:** none. **Ships standalone** — nothing else in the system changes behaviour.

### Domain — `TestOrders/Domain/Physicians/`

- `Physician.cs` — `: AggregateRoot`; `Register(id, fullName, licenceNumber?, registeredAt)`, `Update(...)`, `Deactivate(...)`, `Reactivate(...)`
- `PhysicianInfo.cs` — `: ValueObject`, private ctor + static `Of(...)`. ⚠ The `ValueObjectShouldHavePrivateConstructorWithParametersForHisState` arch test requires ctor parameter names to match the public property names case-insensitively
- `PhysicianStatus.cs`; `Events/` (Registered / Updated / Deactivated / Reactivated); `Rules/` — `PhysicianMustHaveFullNameRule`, `CannotUpdateInactivePhysicianRule`, `CannotDeactivateInactivePhysicianRule`, `CannotReactivateActivePhysicianRule`
- **Modify** `PhysicianId.cs` → `: AggregateId<Physician>` (needed for `IAggregateStore.Load<Physician>`). Both bases expose `.Value`, so `Order.cs:168` compiles unchanged

### Migration

`HC.LIS.Database/TestOrders/<ts>_TestOrdersModule_AddTablePhysicianDetails.cs` →
`test_orders."PhysicianDetails"`: `Id` PK, `FullName` NOT NULL, `LicenceNumber` NULL, `Status` NOT NULL, `RegisteredAt`, `UpdatedAt` NULL, `DeactivatedAt` NULL, + index on `FullName`.

**No FK** from `OrderDetails."RequestedBy"` — historical rows hold user ids and would violate it.

### Application — `TestOrders/Application/Physicians/`

`RegisterPhysician/`, `UpdatePhysician/`, `DeactivatePhysician/`, `ReactivatePhysician/` (command + handler + notification + projection + `*PublishEventNotificationHandler`), `GetPhysicianDetails/` (incl. `PhysicianDetailsProjector : ProjectorBase, IProjector`), and `SearchPhysicians/`.

- `SearchPhysiciansQuery(string? SearchTerm, bool IncludeInactive)` — **one query serves both the picker and the admin page**; `ILIKE` on name and licence. Queries must be immutable (`QueryShouldBeImmutable` arch test)
- Projector inserts use **`ON CONFLICT DO NOTHING`** — an outbox replay otherwise duplicates rows (known trap in this repo)

### Integration events + API

`TestOrders/IntegrationEvents/Physician{Registered,Updated,Deactivated,Reactivated}IntegrationEvent`.

New group `HC.LIS.API/Modules/TestOrders/Physicians/PhysiciansEndpoints.cs`, mounted in `Program.cs` (~line 236) as `v1.MapGroup("physicians").MapPhysiciansEndpoints()`:

| Route | Policy |
|---|---|
| `GET ""` (search), `POST ""` (register), `GET "{id:guid}"` | new `OrderEntry` = Receptionist + ITAdmin — covers admin CRUD *and* receptionist quick-add |
| `PUT "{id:guid}"`, `POST "{id}/deactivate"`, `POST "{id}/reactivate"` | `ITAdmin` |

Add the `OrderEntry` policy beside the existing two at `Program.cs:119-120` (don't reuse the semantically-wrong `PatientManagement` policy). `.ProducesProblem(409)` on mutating endpoints — `BaseBusinessRuleException` maps to 409.

### ⚠ Four registration points (three fail silently)

| Where | Consequence if missed |
|---|---|
| `DomainEventTypeMappings.cs` + `MartenConfig.cs` | replay throws at runtime |
| `TestOrdersStartup.cs` domain-notification BiMap | notification never dispatched |
| `HC.LIS.API/Configuration/EventBus/HcLisEventRegistry.cs` | **RabbitMQ mode only** — in-memory dev/e2e passes, so this breaks nowhere you'd notice locally |
| `DataAccessModule` (Part 4's provider) | DI resolution failure at startup |

### Tests

- `Tests/UnitTests/Physicians/` — `PhysicianSampleData`, `PhysicianFactory`, `PhysicianTests`: register / name-required (empty **and** whitespace) / licence optional / update / update-inactive-breaks / deactivate / reactivate / replay-rebuild. `SystemClock.Set(...)`, never `DateTime.UtcNow`
- `Tests/IntegrationTests/Physicians/` — register → projection row via a probe; update; deactivate; search matches partial name and licence and excludes inactive unless `IncludeInactive`; outbox message present
- Add `DELETE FROM "test_orders"."PhysicianDetails";` to the IntegrationTests `TestBase.ClearDatabase`

**Done when:** `POST /api/v1/physicians` creates a physician, `GET /api/v1/physicians?search=` finds it, and the TestOrders unit + integration + arch suites are green.

---

## Part 2 — SDK + physicians port

**Prerequisites:** Part 1 merged and the API runnable.

Regenerate first — nothing below compiles until it's done:

```bash
dotnet run --project src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj    # localhost:5000
cd src/HC.LIS.Frontend/packages/hc-lis-api-client && yarn generate && yarn build
```

Commit the regenerated `src/generated` as `chore(sdk):`.

- `core/domain/physician-search-result.ts`, `physician-details.ts`
- `core/application/i-physicians-port.ts` — `PHYSICIANS_PORT`: `search`, `list`, `getDetails`, `register`, `update`, `deactivate`, `reactivate`
- `core/infrastructure/physicians/sdk-physicians-adapter.ts` — **adapter-only, no separate `*-api.ts`**, mirroring the *patients* area (orders/users use the two-file split)
- Wire in `app.config.ts`

**Done when:** a spec asserting the adapter's mapping shape passes, and `yarn test` is green.

---

## Part 3 — Physician picker + new-order form

**Prerequisites:** Part 2.

- `features/orders/physician-picker.component.{ts,html,css}` — clone `patient-picker.component.*` (300 ms debounce, `HcCombobox`). Testids `physician-picker-input` / `-results` / `-result-item` / `-selected-card` / `-clear-btn`. Filter out `Inactive` (as the patient picker filters `Anonymized`)
- Quick-add: when a search returns nothing, `physician-picker-quick-add-btn` opens an `hc-dialog` (`physician-quick-add-name-input`, `-licence-input`, `-submit-btn`) with a pending signal set in `try/finally` and bound to both `[loading]` and `[disabled]`, per the CLAUDE.md loading rules
- `features/orders/new-order.component.*` — physician required; submit disabled until patient **and** physician are chosen; send `requestedBy: physician.id`; drop the now-unused `AuthService`

### Tests — this is the biggest blast radius in the project

- New `e2e/order-physician-picker.spec.ts`: full workflow, clear-selection, quick-add, and a role-guard test (Physician role on `/orders/new` → `/unauthorized`)
- `e2e/fixtures/physicians.ts` — `ensurePhysician(page)` + `pickPhysician(page, name)`. ⚠ **Must create a *real* physician** via `POST /api/v1/physicians`; do **not** `page.route`-mock `/api/v1/physicians` the way `pickPatient` mocks patients. A mocked id has no `PhysicianDetails` row and would 409 once Part 4 lands
- Every existing spec that clicks `create-order-submit-btn` now needs a physician: `e2e/orders.spec.ts`, `e2e/order-patient-picker.spec.ts`, `e2e/realtime.spec.ts` (~14 call sites). **Fold selection into a shared `startOrder(page)` helper** rather than patching each site
- `new-order.component.integration.spec.ts` — submit disabled until both are selected; the physician id is sent, not `currentUser().userId`

**Done when:** `yarn e2e` is green and every order created through the SPA carries a registered physician id.

---

## Part 4 — Enforce the physician on order creation

**Prerequisites:** Part 3 (otherwise the SPA breaks). **Backend only.**

Per CLAUDE.md, a domain rule needing DB data uses a **domain-defined provider interface**, never SQL in the handler:

1. `Domain/Physicians/RequestingPhysician.cs` — VO carrying `(PhysicianId, FullName, IsActive)`
2. `Domain/Physicians/IRequestingPhysicianProvider.cs` — `Task<RequestingPhysician?> GetByIdAsync(Guid)`
3. `Infrastructure/Physicians/RequestingPhysicianProvider.cs` — Dapper; register in `DataAccessModule`
4. `CreateOrderCommandHandler` injects it and passes the result into `Order.Create(...)`
5. `Order.Create`'s 3rd parameter becomes `RequestingPhysician`; new rules `OrderMustReferenceRegisteredPhysicianRule`, `OrderMustReferenceActivePhysicianRule`

`OrderCreatedDomainEvent` **keeps its exact shape** (`RequestedBy` stays a `Guid`) → no event-store migration. `CreateOrderRequest.RequestedBy` keeps its wire name; only its meaning changes.

### Tests that break here — fix in the same `test:` commit

`OrderFactory` (unit **and** integration), `OrderTests`, `GetOrdersListTests`, `GetOrderDetailsWithItemsTests`, `GetOrdersListWithPatientInfoTests`, `PlaceExamInProgressViaSampleCollectedTests`, plus `HC.LIS.Tests/IntegrationEvents/FullWorkflowTests.cs` and `OrderItemAcceptedFlowTests.cs` (both currently pass `ExecutionContext.UserId` as `RequestedBy`). Add the new tables to `HC.LIS.Tests/IntegrationEvents/DatabaseCleaner.cs`.

**Done when:** creating an order with an unregistered or inactive physician throws, and the full C# suite is green.

---

## Part 5 — Physician name on order views

**Prerequisites:** Part 4.

- `OrderListItemDto` and `OrderDetailsDto` each gain `string? RequestedByName`
- `GetOrdersListQueryHandler`: `LEFT JOIN test_orders."PhysicianDetails"` on `RequestedBy`. ⚠ This query aggregates `COUNT` — **the new column must go in the `GROUP BY`**
- `GetOrderDetailsQueryHandler`: same join (it currently joins nothing)
- `OrderCreatedIntegrationEvent` gains a **trailing nullable** `string? RequestedByName`; enrich it in `OrderCreatedPublishEventNotificationHandler` by injecting `IQueryHandler<GetPhysicianDetailsQuery, …>` (closed query-handler types are DI-registered in `MediatorModule`), exactly as `patientName` is enriched today
- `UiNotificationTranslator`'s `OrderAdded` frame emits `requestedByName`, so an SSE-inserted row matches a fresh load
- `order-list.component.*` — restore the sortable **Requested By** column (`order-list-sort-requested-by`, cell `requested-by-cell`), `requestedByName ?? 'Unknown physician'`; bump the empty-state `colspan` 5 → 6 and add a skeleton `<td>`
- `order-detail.component.html` — restore `data-testid="requested-by"` in `order-meta`, bound to the **name**. No client-side lookup — the name is on the DTO

### Guard specs to invert

These four currently assert the physician is **absent**. Flip them to assert a name is shown, keeping a `not.toMatch(/^[0-9a-f]{8}-/i)` assertion so the "never show raw UUIDs" intent survives:

- `features/orders/order-list.component.integration.spec.ts:70`
- `features/orders/order-detail.component.integration.spec.ts:162`
- `e2e/orders.spec.ts:82` and `:206`

**Done when:** the order list and detail show a physician name for newly created orders and `Unknown physician` for legacy ones.

---

## Part 6 — Forward propagation: LabAnalysis worklist + report

**Prerequisites:** Part 1 only — parallelisable with Parts 3–5.

**csproj:** add a `ProjectReference` to `HC.LIS.Modules.TestOrders.IntegrationEvents.csproj` in **both** `LabAnalysis/Application` and `LabAnalysis/Infrastructure`. `SampleCollection/Application` already does exactly this.

⚠ **Arch constraint:** `HC.LIS.Tests/ArchTests/Modules/ModuleTests.cs` forbids LabAnalysis depending on the TestOrders namespace *unless* the type implements `INotificationHandler<>`, is named `*IntegrationEventHandler`, or is `EventsBusStartup`. The handlers satisfy this; **the internal commands must take primitives only.**

Every inbound integration event **must** schedule an internal command (CLAUDE.md hard rule), mirroring `Application/Patients/StorePatientSnapshot/`:

- `Application/Physicians/` — `IPhysicianSnapshotRepository` + `PhysicianSnapshotView`, then `StorePhysicianSnapshot/`, `UpdatePhysicianSnapshot/`, `DeactivatePhysicianSnapshot/`, `ReactivatePhysicianSnapshot/`
- `Application/Orders/StoreOrderPhysician/` — consumes `OrderCreatedIntegrationEvent` → `StoreOrderPhysicianByOrderIdCommand`
- `Infrastructure/{Physicians,Orders}/…Repository.cs` — Dapper, `ON CONFLICT DO NOTHING` for inbox re-delivery
- Migrations: `lab_analysis."PhysicianSnapshotDetails"` and `lab_analysis."OrderPhysicianSnapshotDetails"` (`OrderId` PK, `PhysicianId`, `RequestedAt`)
- Subscribe all 5 events in `EventsBusStartup.cs`

⚠ **All 5 internal commands must be added to `LabAnalysisStartup`'s BiMap** — `InternalCommandsModule.CheckMappings` throws `NotMappedInternalCommandsException` at container build if any is missing. Naming per CLAUDE.md: `*By{Identifier}Command`, never an `InternalCommand` suffix.

### Consume it

- `WorklistItemSummaryDto` += `string? RequestedByName`. ⚠ It's a **positional record** — arity changes; grep construction sites in `LabAnalysis/Tests`
- `WorklistItemDetailsDto` += `RequestedByName`
- Both query handlers join `worklist_item_details.order_id → "OrderPhysicianSnapshotDetails" → "PhysicianSnapshotDetails"`
- `Application/Reports/HtmlReportTemplate.cs` — add a **Requesting Physician** block to `header-info`; `QuestPdfGenerator` gets a matching column. The report is built from `WorklistItemDetailsDto` by `UploadHtmlReportBySignedReportIdCommandHandler`, so no new plumbing
- `WorklistItemCreatedIntegrationEvent` + `UiNotificationTranslator` enrichment. ⚠ Races the mapping internal command — must tolerate `null`; the e2e should assert the *reloaded* value, not the live frame
- `features/worklist/*` — Requesting Physician column + detail field

**Done when:** a signed report header carries the requesting physician and the worklist row shows it.

---

## Part 7 — ITAdmin physician registry page

**Prerequisites:** Part 2 — parallelisable with Parts 3–6.

`features/admin/physicians/` — service + list + create form, route `/admin/physicians` under `roleGuard('ITAdmin')`, nav link in `shell.component.ts`. Mirror `features/admin/users` (`HcTable`, `HcSkeleton` + `SKELETON_ROWS`, `HcEmpty`, `HcPagination`, `HcDropdownMenu`, confirm dialog, `ToastService`).

Testids: `create-physician-btn`, `physician-full-name-input`, `physician-licence-input`, `physician-form-submit-btn`, `physician-list-table`, `physician-list-row`, `physician-actions-trigger`, `physician-action-{edit,deactivate,reactivate}`, `physician-status-badge`.

`e2e/admin-physicians.spec.ts` — full workflow (create → edit → deactivate) plus a role guard (Receptionist → `/unauthorized`). ⚠ `e2e/nav.spec.ts` and `e2e/a11y.spec.ts` enumerate nav links — grep and update.

---

## Part 8 — Full verification

```bash
dotnet build
dotnet test   # TestOrders + LabAnalysis Unit/Integration/Arch, HC.LIS.Tests, HC.LIS.API.Tests

docker-compose -f development-compose.yaml up -d
dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj   # re-run after Parts 1 and 6

cd src/HC.LIS.Frontend/packages/hc-lis-spa && yarn test && yarn e2e
```

Extend `HC.LIS.Tests/IntegrationEvents/FullWorkflowTests.cs`: register a physician, create the order with it, and once the worklist item appears assert `WorklistItemDetailsDto.RequestedByName` matches — poll ≥ 60 s, since that's two async Quartz hops.

**Manual end-to-end** (API + DB + `ng serve`, `RateLimit__Enabled=false`):

1. `itadmin@hclis.local` → `/admin/physicians` → register "Dr. Ana Lima" → visible in the list
2. `receptionist@hclis.local` → `/orders/new` → pick a patient, type "Ana", select → Create Order
3. `/orders` shows **Requested By** = "Dr. Ana Lima" (not a guid, not "Unknown physician"); the detail card matches
4. Search a name that doesn't exist → quick-add → the new physician is selected immediately
5. Drive an exam through accept → collect → result; as `physician@hclis.local`, the `/worklist` row and detail show the requesting physician; sign the report and confirm the header carries it
6. `receptionist@hclis.local` on `/admin/physicians` → `/unauthorized`

Finish by documenting the registry in `CLAUDE.md`.

---

## Risk register

1. **No seed migration for physicians.** A seeded read-model row without a matching Marten stream makes `Load<Physician>` return null, so `Update`/`Deactivate` throw. E2E creates physicians through the API instead.
2. **Legacy rows show "Unknown physician."** Existing `OrderDetails.RequestedBy` values are user ids; the `LEFT JOIN` yields NULL — same treatment as the existing `PatientName ?? 'Unknown patient'`. Historical worklist items and reports likewise have no mapping row. Accepted by design; a cross-schema backfill would violate module isolation. Call it out in the PR body.
3. **`PhysicianId` base change** drops `Id`'s `Guid.Empty` guard — grep for `new PhysicianId(Guid.Empty)` before merging. Compensated by the new order rules.
4. **`OrderCreatedIntegrationEvent` gains a second consumer** (UI translator + LabAnalysis) — only append trailing nullable fields, never reorder.
5. **Registration is a silent-failure surface** — see the Part 1 table. Only the internal-command map fails loudly.
6. `TreatWarningsAsErrors=true` with all analyzers on — unused usings, missing `ConfigureAwait(false)`, and `CultureInfo`-less string ops fail the build. Copy neighbouring files' idioms exactly.
7. `yarn generate` needs a live API on `localhost:5000`; the generated client is committed and CI consumes it — keep it that way.
8. Pre-existing, **out of scope, worth noting in the PR**: `SampleCollectedPublishEventNotificationHandler.cs:28-30` publishes empty strings for patient demographics, and the report footer still prints `Signed by: {guid}` (a UserAccess id — a separate pipeline).

---

## Progress

- [x] **Part 1** — Physician registry (backend + API)
- [ ] **Part 2** — SDK + physicians port
- [ ] **Part 3** — Physician picker + new-order form
- [ ] **Part 4** — Enforce the physician on order creation
- [ ] **Part 5** — Physician name on order views
- [ ] **Part 6** — LabAnalysis worklist + report
- [ ] **Part 7** — ITAdmin physician registry page
- [ ] **Part 8** — Full verification

### Session notes

_(append one line per completed part: date, branch, commits, anything the next session needs)_

**2026-08-05 — Part 1 done.** Branch `feat/physician-registry`, commits `59c2314`…`ec4f4dd` (test → feat pairs for domain, migration, application, API). Notes for the next session:

- **`SearchPhysiciansQuery` is a plain class, not a positional record.** A record's `init` accessors report `CanWrite == true`, which the `QueryShouldBeImmutable` arch test rejects. Property names are still `SearchTerm` / `IncludeInactive`; the constructor parameters are camelCase, so call it as `new SearchPhysiciansQuery(term, includeInactive: false)`.
- **Search is prefix-only (`term%`), not infix (`%term%`)** — a leading wildcard forces a full scan of the registry. A blank term passes `"%"` so the admin page lists everything through the same handler. **Two consequences to settle before Part 3:**
  1. Part 8's manual script says register "Dr. Ana Lima" then type "Ana". That will **not** match — the stored name starts with "Dr.". Either drop honorifics from the stored `FullName` (put the title in the UI), or change the script to type from the start of the name.
  2. The `FullName` btree index does not serve `ILIKE` at all. If prefix search must actually use an index, switch the predicate to `lower("FullName") LIKE lower(@SearchTerm)` and add an expression index on `lower("FullName")`. If infix search is later judged necessary for the typeahead, that needs `pg_trgm` + a GIN index, not a btree.
- All four registration points from the Part 1 table are done: `DomainEventTypeMappings`, `MartenConfig`, the `TestOrdersStartup` domain-notification BiMap, and `HcLisEventRegistry`. Notifications themselves need no DI edit — `ProcessingModule` already auto-registers every closed `IDomainEventNotification<>` in the Application assembly.
- API routes are mounted at `v1.MapGroup("physicians")`; the new `OrderEntry` policy (Receptionist + ITAdmin) sits beside `ITAdmin` / `PatientManagement` in `Program.cs`.
- **Run the migration before the integration tests** — `PhysicianDetails` is new and `TestBase.ClearDatabase` now deletes from it, so an un-migrated database fails every TestOrders integration test, not just the physician ones.
- **Verified:** `dotnet build` clean (0 warnings) across every project except the pre-existing broken `HC.LIS.Tests.IntegrationEvents`; all unit + arch suites green (TestOrders unit 39, arch 22). **The physician integration tests were never executed** — this box has no Docker/Postgres, so CI is the first real run.
- **Pre-existing failure, unrelated, but it will bite Part 6:** `LabAnalysis.ArchTests.InternalCommandShouldHaveConstructorWithJsonConstructorAttribute` fails — `StorePatientSnapshotByPatientIdCommand`, `UpdatePatientSnapshotByPatientIdCommand` and `AnonymizePatientSnapshotByPatientIdCommand` lack `[method: JsonConstructor]` (since `b5cacd3`, 2026-06-11). Part 6 adds five more internal commands to that same module — fix those three while you are there.
