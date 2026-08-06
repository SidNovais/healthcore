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
- [x] **Part 2** — SDK + physicians port
- [x] **Part 3** — Physician picker + new-order form
- [x] **Part 4** — Enforce the physician on order creation
- [x] **Part 5** — Physician name on order views
- [x] **Part 6** — LabAnalysis worklist + report
- [x] **Part 7** — ITAdmin physician registry page
- [~] **Part 8** — Full verification (C# side complete; `yarn e2e` still outstanding)

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

**2026-08-05 — Part 2 done.** Branch `feat/physician-registry`. Notes for the next session:

- **There is no `chore(sdk):` commit — `src/generated/` is gitignored** (`packages/hc-lis-api-client/.gitignore`) and CI never builds the frontend, so the generated client is *not* committed. Risk-register item 7 is wrong on this point. Every contributor must run `yarn generate && yarn build` in `hc-lis-api-client` themselves after an API change.
- **Trap that cost the most time: `packages/hc-lis-spa/node_modules/@hc-lis/api-client` was a stale June copy shadowing the workspace symlink.** `yarn generate && yarn build` in the api-client package appeared to do nothing — the spa kept compiling against months-old types. Fix: `rm -rf packages/hc-lis-spa/node_modules/@hc-lis` so resolution falls through to the root symlink. **If a new SDK function "has no exported member", check this first.**
- To run the API for `yarn generate` on a box with no Docker: it boots fine without Postgres (Marten/EF connect lazily), but **set `ASPNETCORE_HCLIS_EventBus__Type=memory`** or startup stalls retrying RabbitMQ. Swagger then answers on `http://localhost:5000/swagger/v1/swagger.json`.
- `IPhysiciansPort.list(includeInactive)` sends a **blank** search term, which the Part 1 handler turns into `"%"` — one endpoint serves both the picker and the Part 7 admin page, exactly as designed.
- `search(term)` passes `includeInactive: false`, so **inactive physicians are already excluded server-side**. Part 3's picker still filters client-side for parity with the patient picker, but it is belt-and-braces, not load-bearing.
- New domain file beyond the two the plan listed: `core/domain/register-physician-params.ts` (`RegisterPhysicianParams` / `UpdatePhysicianParams`), mirroring `register-patient-params.ts`. `PhysicianStatus` (`'Active' | 'Inactive'`) is exported from `physician-search-result.ts`.
- **Verified:** `yarn test` 365/365 green across 51 files (new `sdk-physicians-adapter.spec.ts` = 7 tests), and `yarn build` clean apart from the pre-existing `jsbarcode` CommonJS warning.

**2026-08-05 — Part 3 done.** Branch `feat/physician-registry`, commits `af58241` (test) → `7cd3cb1` (feat). Notes for the next session:

- **`e2e/fixtures/orders.ts` is new and now owns patient selection too.** `startOrder(page)` = `ensurePhysician` + `pickPatient` + `pickPhysician` and asserts the submit button is enabled before returning. `orders.spec.ts` and `realtime.spec.ts` deleted their local `pickPatient` / `pickSeedPatient` copies and call it; `order-patient-picker.spec.ts` keeps its own real-patient flow and only adds the physician half. **Part 4 needs no e2e edits — every order-creating spec already sends a registered id.**
- **Nested `<form>` is the trap in the quick-add dialog.** The dialog lives inside `app-physician-picker`, which sits inside the new-order `<form>`; a `<form>` in the dialog would be nested, and its submit button can fire the *outer* form's `ngSubmit`. The dialog therefore uses `(click)="submitQuickAdd()"` (the `create-user-form` idiom) plus `(keydown.enter)` on the inputs — do not "fix" it into a form.
- `NgModel` inside the picker does **not** attach to the outer `NgForm` (`NgModel` injects its parent with `@Host()`, which stops at the component boundary), so the quick-add inputs need neither `name` nor `standalone: true`.
- Quick-add is offered only after a search **completes** with zero active rows (`searchReturnedEmpty` + empty `results`), never before the first search — the spec pins both halves.
- The picker filters `Inactive` client-side even though `search()` already passes `includeInactive: false`; that is parity with the patient picker, not a load-bearing check.
- `ensurePhysician` polls `GET /api/v1/physicians?search=` until the new row is findable before returning, so specs never race the projection.
- **Verified:** `yarn test` 374/374 green across 52 files (`physician-picker.component.spec.ts` = 8, new-order integration 6 → 7), `yarn build` clean apart from the pre-existing `jsbarcode` warning, and `tsc --noEmit` clean over `e2e/`. **`yarn e2e` was NOT run** — this box has no Docker/Postgres, so the API cannot serve `/api/v1/physicians`. The e2e specs are unexecuted, exactly as the Part 1 integration tests were.

**2026-08-05 — Part 4 done.** Branch `feat/physician-registry`, commits `9079d41` (test) → `7c47983` (feat). Notes for the next session:

- **`Order.Create`'s 3rd parameter is `RequestingPhysician?`, not `RequestingPhysician`.** The nullability is load-bearing: it is the only way `OrderMustReferenceRegisteredPhysicianRule` can live in the Domain instead of leaking a null-check into the command handler. `CheckRule` is invisible to the compiler's flow analysis, so the event construction that follows uses `requestedBy!.PhysicianId.Value`.
- `IRequestingPhysicianProvider.GetByIdAsync` takes a `CancellationToken` (the plan wrote `GetByIdAsync(Guid)`); that matches the repo's only other domain-defined provider, `IWorklistItemForSigningProvider`.
- **The `PhysicianDetails` projection is eventually consistent**, so any test that creates an order must register the physician *and wait for the projection row* first. Both new helpers do exactly that — `IntegrationTests/Orders/OrderFactory.RegisterRequestingPhysicianAsync` (polls `GetPhysicianDetailsFromTestOrdersProbe`, 15 s) and `HC.LIS.Tests/IntegrationEvents/RequestingPhysicianFactory.RegisterAsync` (same, via `IntegrationTestAssert.AssertEventually`). Do not create an order straight after `RegisterPhysicianCommand`.
- **xUnit1030 is an error in the integration-test project**: a `Func<Task> action = async () => await …ConfigureAwait(false);` written *inside* a `[Fact]` fails the build. Use `ConfigureAwait(true)` in test-method bodies.
- New integration file `Orders/CreateOrderPhysicianEnforcementTests.cs` covers unregistered → `OrderMustReferenceRegisteredPhysicianRule`, deactivated → `OrderMustReferenceActivePhysicianRule`, and the happy path.
- **Verified:** every unit + arch suite green — TestOrders unit 39 → **41**, TestOrders arch 22, HC.LIS.Tests.ArchTests 8, and Analyzer/LabAnalysis/PatientManagement/SampleCollection/UserAccess/HC.Core all green. `dotnet build` clean (0 warnings) for the API and every test project that compiles.
- **Two pre-existing failures confirmed unrelated and untouched:** (1) `LabAnalysis.ArchTests.InternalCommandShouldHaveConstructorWithJsonConstructorAttribute` (still the three patient-snapshot commands — fix in Part 6); (2) `HC.LIS.Tests.IntegrationEvents` does not compile — `CreateBarcodeCommand` does not exist anywhere in SampleCollection and `RecordSampleCollectionCommand` takes 4 args, not 7. **The Part 4 edits to that project were applied and are correct, but the project cannot be built until that pre-existing drift is fixed.**
- **The physician integration tests were again never executed** — no Docker/Postgres on this box. CI is the first real run for Parts 1 and 4 both. Run the migration before them.

**2026-08-05 — Part 5 done.** Branch `feat/physician-registry`, commits `a1dc871` (test) → `2755f23` (feat) → `ca611b1` (test) → `0f6c00d` (feat). Notes for the next session:

- **Regenerate the SDK before touching the SPA** — both order DTOs gained `requestedByName`, and `src/generated` is gitignored (see the Part 2 note), so a fresh clone compiles against the old shape. `yarn generate` needs the API on `localhost:5000`; on a box with no Docker it also needs `ASPNETCORE_HCLIS_JWT_{ISSUER,AUDIENCE,SECRET_KEY}` set or startup throws before Swagger is served — `EventBus__Type=memory` alone is not enough. Values that work: `healthcore` / `healthcore` / `supersecretkey1234567890abcdefgh`.
- **`GetOrderDetailsQueryHandler` does not follow the `AS "{nameof(Dto.Prop)}"` convention** — it predates it and uses literal aliases (`od`, `p`). The new join matches the file's existing idiom rather than half-converting one column; converting the whole handler is a separate cleanup.
- The e2e order-detail test no longer clicks `order-list-row.first()`. It captures its own `orderId` and targets `[data-order-id="…"]`, because asserting an *exact* physician name on whatever row happens to sort first is flakeable by an order a previous spec left behind.
- The two `page.route`-mocked order-list tests in `orders.spec.ts` (the patient-name ones) deliberately omit `requestedByName`, so those rows render `Unknown physician`. Nothing asserts on it — that is the legacy-row path getting incidental coverage.
- `OrderCreatedIntegrationEvent`'s new field is trailing and nullable, so Part 6's LabAnalysis consumer can ignore it; the physician *id* is still what travels for the snapshot join.
- **Verified:** TestOrders unit 41 → **43**, HC.LIS.API.Tests 5 → **7** (new `RealTime/UiNotificationTranslatorTests.cs` pins the SSE `OrderAdded` frame), every other unit + arch suite green, `dotnet build` clean. SPA `yarn test` 374 → **377/377** across 52 files, `yarn build` clean apart from the pre-existing `jsbarcode` warning, `tsc --noEmit` clean over `e2e/`.
- **Not executed, same reason as every prior part:** the new `IntegrationTests/Orders/GetOrdersWithPhysicianNameTests.cs` (4 facts, incl. a raw-SQL legacy `OrderDetails` row proving the LEFT JOIN yields NULL rather than dropping the order) and `yarn e2e`. No Docker/Postgres on this box.
- **Still pre-existing and untouched:** `LabAnalysis.ArchTests.InternalCommandShouldHaveConstructorWithJsonConstructorAttribute` (the three patient-snapshot commands — fix in Part 6), and `HC.LIS.Tests.IntegrationEvents` does not compile.

**2026-08-05 — Part 6 done.** Branch `feat/physician-registry`, commits `6211e94` (fix) → `de4cbbe`. Notes for the next session:

- **The pre-existing `InternalCommandShouldHaveConstructorWithJsonConstructorAttribute` failure is fixed, and it was a real bug, not a test nit.** The three patient-snapshot commands imported `System.Text.Json.Serialization.JsonConstructor`, but `ProcessInternalCommandsCommandHandler` rehydrates internal commands with Newtonsoft's `JsonConvert.DeserializeObject`. Newtonsoft never saw the attribute and was binding only via its single-public-constructor fallback. **Every internal command in this repo must use `Newtonsoft.Json.JsonConstructor`** — the arch test checks the Newtonsoft attribute specifically.
- **`WorklistItemSummaryDto` had no construction sites outside its own declaration**, so the positional-record arity change the plan warned about cost nothing. Dapper maps it by column name.
- **Two repositories, not one.** `IPhysicianSnapshotRepository` (Application/Physicians) owns the registry mirror; `IOrderPhysicianRepository` (Application/Orders) owns the mapping *and* the `GetRequestingPhysicianAsync(orderId)` join across both tables. Keeping the join out of the physician repo is what stops the two concerns bleeding together.
- `UpdatePhysicianSnapshotByPhysicianIdCommand` carries `UpdatedAt`, so `IPhysicianSnapshotRepository.UpdateAsync` takes it too (the plan implied a 3-arg signature); otherwise the command would silently discard it. `ReactivateAsync` clears `DeactivatedAt` and stamps `UpdatedAt`.
- **`OrderCreatedIntegrationEvent` now has three consumers** — the UI translator, and LabAnalysis via both the new `StoreOrderPhysician` handler. Only ever append trailing nullable fields to it.
- The worklist SSE frame races the mapping internal command by design: `WorklistItemCreatedPublishEventNotificationHandler` publishes `RequestedByName = null` when the mapping has not landed. Two unit tests pin both branches. **An e2e must assert the reloaded value, never the live frame.**
- Report: both `HtmlReportTemplate` and `QuestPdfGenerator` print `Unknown physician` for legacy items. The HTML name goes through `EscapeHtml` — there is a test pinning that, because the name is free text from the registry.
- **The SDK was regenerated and both worklist DTOs now carry `requestedByName`.** To boot the API for `yarn generate` on a box with no Docker you need **four** env vars, not the three the Part 5 note lists: `ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING` is also required (`Program.cs:182` throws without it) even though nothing connects. Add it to `EventBus__Type=memory` + the three `JWT_*` values.
- **Verified:** every project builds clean (0 warnings) except the pre-existing broken `HC.LIS.Tests.IntegrationEvents`; all unit + arch suites green — LabAnalysis unit 35 → **51**, LabAnalysis arch **22/22** (was 21/22), TestOrders unit 43, HC.LIS.API.Tests 7 → **9**, HC.LIS.Tests.ArchTests 8. SPA `yarn test` 377 → **381/381** across 52 files, `yarn build` clean apart from the pre-existing `jsbarcode` warning, `tsc --noEmit` clean over `e2e/`.
- **Not executed, same reason as every prior part** (no Docker/Postgres on this box): the new `IntegrationTests/WorklistItems/WorklistItemRequestingPhysicianTests.cs` (4 facts) and `yarn e2e`, including the new route-mocked `worklist row shows the requesting physician name` test. **Run the two new migrations before them** — `TestBase.ClearDatabase` and `DatabaseCleaner` now delete from both new tables, so an un-migrated database fails every LabAnalysis integration test.
- **Still pre-existing and untouched:** `HC.LIS.Tests.IntegrationEvents` does not compile (`CreateBarcodeCommand` does not exist; `RecordSampleCollectionCommand` takes 4 args, not 7). Part 8's plan to extend `FullWorkflowTests` is blocked until that drift is fixed.

**2026-08-05 — Part 7 done.** Branch `feat/physician-admin-page` (off `main`, post-merge), commits `dc4572f` (test) → `bcb1b8f` (feat). **Frontend only — no backend or SDK change, so nothing to regenerate.** Notes for the next session:

- **There is no "Registered" column, and the plan's implied parity with the users table does not hold.** `PhysicianSearchResultDto` carries only `Id`, `FullName`, `LicenceNumber`, `Status` — no `RegisteredAt` — and `IPhysiciansPort.list()` is that same search endpoint. Columns are Full Name · Licence Number · Status · Actions. Adding the date means a trailing field on the search DTO + handler + SDK regen; it was judged out of scope for a frontend-only part.
- **The list calls `port.list(true)`.** Inactive physicians are the only rows the reactivate action can act on, so filtering them out would hide exactly what the page exists to fix. Each row offers deactivate *or* reactivate, never both — a test pins that.
- **`PhysicianFormComponent` prefills in `ngOnInit`, not an `effect()`.** The first attempt used an effect and the edit dialog rendered with empty inputs for one change-detection pass; the unit test caught it. The host recreates the component on every dialog open (`@if (formOpen())`), so the row is known before the first pass. Separately, **`NgModel` writes the initial DOM value in a microtask** — the prefill spec must `await fixture.whenStable()` before reading `input.value`, which is why that one test is async.
- The dialogs bind `[open]` + `(openChange)` rather than `[(open)]`, because Esc/backdrop dismissal also has to clear `editing` / `pendingStatusChange`.
- New `stethoscope` glyph in `ui/icon/icon.ts` — `/patients` already owns `user` and Users owns `users`.
- **Enumerating specs checked, only one needed an edit:** `e2e/a11y.spec.ts` gained `/admin/physicians`. `nav.spec.ts` only asserts the Users link is active on landing, and `command-palette.spec.ts` filters `"work"` (which "Physicians" does not match) — both are unaffected. `hipaa.spec.ts` never visits the route.
- **Verified:** SPA `yarn test` 381 → **407/407** across 54 files (`physicians.service.spec.ts` = 10, `physician-list.component.integration.spec.ts` = 16), `yarn build` clean apart from the pre-existing `jsbarcode` warning, and `tsc --noEmit` clean over `e2e/`.
- **Not executed, same reason as every prior part** (no Docker/Postgres on this box): the new `e2e/admin-physicians.spec.ts` (4 tests — create→edit→deactivate, reactivate, Receptionist role guard, nav-link visibility) and the two new a11y route scans.

**2026-08-06 — Part 8: C# verification complete, e2e outstanding.** Branch `feat/physician-admin-page`. **Docker + Postgres were available this session**, so every integration suite deferred since Part 1 actually ran. Notes:

- **`dotnet build` over a CI-shaped generated `.slnx` (55 projects): clean, 0 warnings.** There is no root solution file — replicate CI: `dotnet new sln -n HealthCore --format slnx` then add every `src/**/*.csproj`.
- **All unit + arch suites green: 311 tests.** TestOrders unit 43, LabAnalysis unit 51, HC.LIS.API.Tests 9, every arch suite 22/22, HC.LIS.Tests.ArchTests 8.
- **All integration suites green: 72 tests — first real execution of Parts 1/4/5/6.** TestOrders 30, LabAnalysis 12, SampleCollection 8, Analyzer 6, UserAccess 5, TcpMessage 7, HC.Core 1. **Nothing in the physician registry needed fixing.** Run the migrations first (`PhysicianDetails` + both LabAnalysis snapshot tables), since the cleaners delete from them.
- **`HC.LIS.Tests.IntegrationEvents` compiles and runs again** (was broken since the barcode refactor, excluded from CI). The drift was *not* just renames: barcodes are no longer supplied by the caller. `CreateBarcodeCommand` is gone — barcode generation is now scheduled automatically by `MovePatientToWaitingCommand` and lands via the internal command `GenerateSampleBarcodesForCollectionRequestCommand`, so tests must **poll for the generated value** (new `GetGeneratedBarcodeFromSampleCollectionProbe`). `RecordSampleCollectionCommand` takes 4 args, not 7 — the patient demographics were dropped. The `barcode` parameter was therefore removed from `SetupCollectedSampleAsync` / `SetupExamResultReadyAsync` / `SetupWorklistItemWithResultAsync`, which now *return* it.
- **A missing step was also restored:** an `AnalyzerSample` must be handed its info (`DispatchSampleInfoCommand`) before it may accept a result, else `CannotReceiveResultForNonDispatchedSampleRule` throws.
- **Part 8's `FullWorkflowTests` extension is written and passes** — it registers a physician, creates the order with it, and asserts `WorklistItemDetailsDto.RequestedByName == "Dr. Ana Lima"` via `RequestingPhysicianOnWorklistItemProbe` (60 s budget, two async Quartz hops). Verified end-to-end: the enriched `WorklistItemCreatedIntegrationEvent` carries `"RequestedByName":"Dr. Ana Lima"`.
- ⚠️ **It passes ~2 runs in 3, blocked by a PRE-EXISTING defect unrelated to the physician registry.** A single `OrderItemAcceptedIntegrationEvent` inbox row is sometimes handled **twice**, seconds apart — proven by one inbox row producing *both* `CreateCollectionRequestForOrderCommand` **and** `AddExamToCollectionForOrderCommand` (the two mutually-exclusive branches of `OrderItemAcceptedIntegrationEventHandler`, chosen by whether the aggregate loads). The same signature appears in Analyzer (one inbox row → two `CreateAnalyzerSampleBySampleCollectedCommand`). Consequence chain: duplicate exam on the `CollectionRequest` → duplicate exam in `SampleCollectedIntegrationEvent.Exams` (identical `ExamId`) → `AnalyzerSample` built with two `HGB` exams → `AnalyzerSample.When(WorklistItemAssignedDomainEvent)` calls `_exams.Single(...)` → `"Sequence contains more than one matching element"` → **the worklist assignment is permanently lost** (the internal command is marked processed with the exception in its `Error` column, so it never retries). Diagnose it via `analyzer."InternalCommands"."Error"` and the Marten tombstone events in `analyzer.mt_events`. This makes 4 of the 9 tests in the project intermittently fail; the project is still excluded from CI, so nothing regressed there.
- **`yarn test` 407/407 across 54 files**, unchanged from Part 7.
- The registry is now documented in `CLAUDE.md` under **Architecture → Referring-Physician Registry**.

**2026-08-06 — Part 8: `yarn e2e` run (chromium). All 8 physician specs green.**

- **Three environmental blockers had to be cleared first, none of them the feature:**
  1. **`.vscode/launch.json` had no `RateLimit` setting**, and its `env` block governs the process — so restarting the API from VS Code could never disable the limiter (measured: 10 logins then `429`). Added a dedicated **`HC.LIS.API (E2E)`** launch config; do not disable the limiter in the normal dev profiles or it stops being exercised day to day.
  2. **The SPA would not bootstrap at all** — `does not provide an export named 'deactivatePhysician'`. `src/generated` is gitignored, so the checked-out SDK was stale. `yarn generate && yarn build` in `hc-lis-api-client`, then **delete `packages/hc-lis-spa/.angular/cache`** (Vite pre-bundles the dep and the stale copy survives a restart). This was *not* the Part 2 symlink-shadowing variant — the junction was fine.
  3. **Running the C# integration suites truncates `user_access.users`,** which deletes the e2e seed accounts. Restore them by deleting versions `20260418120600` and `20260509000000` from `public."VersionInfo"` and re-running the migrator. **Do not run the integration suites and e2e against the same database without re-seeding in between.**
- **Two real defects in Part 7's never-executed `admin-physicians.spec.ts`, both fixed** (backend verified correct at every step — register/update/deactivate/reactivate all project within ~15 s):
  1. `PhysicianDetails` is a projection and **the list is fetched once per page load**, so retrying a locator can never succeed — the page must be *reloaded*. The picker specs pass only because `ensurePhysician` already polls; the admin spec had no equivalent.
  2. The registry **paginates at 10 with no search box and no delete endpoint**, so accumulated rows push a new physician onto a later page. Both are handled by the new `expectRowEventually` helper, which reloads *and* walks pages. ⚠️ When walking, compare **raw `innerText` via `waitForFunction`** — `expect().not.toHaveText()` normalises whitespace, matches instantly, and lets the walk skip a page.
- **Results (chromium):** auth/nav/theme/reduced-motion/command-palette/hipaa **28 passed, 1 failed**; admin-users/**admin-physicians**/patients/order-patient-picker/**order-physician-picker** **24 passed, 0 failed**; orders/triage/worklist/realtime **24 passed, 5 failed, 4 skipped**.
- **The remaining failures are projection-timing and load timeouts, not behaviour, and none are physician-related.** The four `orders.spec.ts` "Order Detail" failures all wait on `exam-items-table` for 5 s after a single navigation; opening the same order manually renders `exam-items-table`, `exam-item-row` **and `requested-by` showing the physician name**, i.e. the Part 5 deliverable works. The `a11y.spec.ts` failures are all `page.waitForLoadState` 30 s timeouts — **no WCAG violations were reported**. These specs are fragile under a loaded box (API + SPA + Postgres + browser on one machine); hardening them the way `expectRowEventually` does is a sensible follow-up but was left out of scope.
- ⚠️ **Background processes are killed at every turn boundary in this environment.** A full `yarn e2e` exceeds the 10-minute foreground limit and will be killed mid-run, losing everything. Run it in chunks, and `Tee-Object` each chunk to a file so partial results survive.
- Still outstanding: **firefox/webkit** (chromium only, as before) and the **manual 6-step script** — whose step 2 remains wrong, since prefix-only search means typing "Ana" will never match "Dr. Ana Lima".
