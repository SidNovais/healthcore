# Implementation task breakdown template

## When to use

Read this template when generating **`docs/specs/{ModuleName}-Tasks.md`** (Artifact 2).

---

## File format

```markdown
# Implementation Tasks: {ModuleName} Module

**Tech Spec:** [docs/specs/{ModuleName}-TechSpec.md](./{ModuleName}-TechSpec.md)
**Date:** {YYYY-MM-DD}

---

## Prerequisites

{List any cross-module changes, integration event enrichments, or infrastructure work that must happen BEFORE this module's tasks begin. If none, write "None — module is self-contained."}

---

## Task List

### Phase 1: Module Skeleton

> Skip this phase if `src/HC.LIS/HC.LIS.Modules/{ModuleName}/` already exists.

- [ ] **Task 1.1** — Scaffold module structure
  - **Skill:** `/create-module {ModuleName}`
  - **Creates:** 7 projects (Domain, Application, Infrastructure, IntegrationEvents, UnitTests, IntegrationTests, ArchTests)
  - **Verify:** `dotnet build` succeeds

---

### Phase 2: Domain Layer (TDD)

{Generate one TEST + IMPLEMENT pair per domain method from TechSpec Section 2.3, in state-machine order.}

- [ ] **Task 2.1** — Write failing unit tests for `{Aggregate}` creation
  - **Skill:** `/unit-test {ModuleName} create a {aggregate}`
  - **Creates:** `Tests/UnitTests/{Aggregates}/{Aggregate}Tests.cs`, `{Aggregate}Factory.cs`, `{Aggregate}SampleData.cs`
  - **Tests:** `Create{Aggregate}IsSuccessful`
  - **Expected:** Tests fail — `{CreatedEvent}` and `{Aggregate}.Create()` do not exist yet

- [ ] **Task 2.2** — Implement `{Aggregate}` aggregate with `Create` method
  - **Skill:** `/domain {ModuleName} create a {aggregate}`
  - **Creates:** `Domain/{Aggregates}/{Aggregate}.cs`, `{Aggregate}Id.cs`, `{Aggregate}Status.cs`, `Events/{CreatedEvent}.cs`
  - **Verify:** Unit tests from Task 2.1 pass

{For each subsequent state transition:}

- [ ] **Task 2.{N}** — Write failing unit tests for `{Method}`
  - **Skill:** `/unit-test {ModuleName} {method description}`
  - **Modifies:** `Tests/UnitTests/{Aggregates}/{Aggregate}Tests.cs`
  - **Tests:** `{Method}IsSuccessful`, `{Method}ShouldBroke{Rule}When{Condition}`
  - **Expected:** Tests fail — `{Event}`, `{Rule}`, and `{Aggregate}.{Method}()` do not exist yet

- [ ] **Task 2.{N+1}** — Implement `{Method}` on `{Aggregate}`
  - **Skill:** `/domain {ModuleName} {method description}`
  - **Creates:** `Events/{Event}.cs`, `Rules/{Rule}.cs`
  - **Modifies:** `Domain/{Aggregates}/{Aggregate}.cs`
  - **Verify:** Unit tests from Task 2.{N} pass

---

### Phase 3: Application Layer — Commands & Handlers

{One task per command, in state-machine order.}

- [ ] **Task 3.1** — Implement `Create{Aggregate}Command` and handler
  - **Creates:** `Application/{Aggregates}/Create{Aggregate}/Create{Aggregate}Command.cs`, `Create{Aggregate}CommandHandler.cs`
  - **Pattern:** Handler uses `IAggregateStore.Start()` for creation

- [ ] **Task 3.2** — Implement `{Aggregate}CreatedNotification` and projection
  - **Creates:** `Application/{Aggregates}/Create{Aggregate}/{Aggregate}CreatedNotification.cs`, `{Aggregate}CreatedNotificationProjection.cs`

{Repeat Task 3.N pairs for each command + notification.}

---

### Phase 4: Application Layer — Read Model

- [ ] **Task 4.1** — Implement read model DTO, query, handler, and projector
  - **Creates:** `Application/{Aggregates}/Get{Aggregate}Details/{Aggregate}DetailsDto.cs`, `Get{Aggregate}DetailsQuery.cs`, `Get{Aggregate}DetailsQueryHandler.cs`, `{Aggregate}DetailsProjector.cs`

---

### Phase 5: Integration Events

- [ ] **Task 5.1** — Define outbound integration events
  - **Creates:** `IntegrationEvents/{EventName}.cs` for each outbound event

- [ ] **Task 5.2** — Implement publish notification handlers
  - **Creates:** `Application/{Aggregates}/{CommandFolder}/{Action}PublishEventNotificationHandler.cs` for each outbound event

- [ ] **Task 5.3** — Implement inbound integration event handler(s)
  - **Creates:** `Application/{Aggregates}/{CommandFolder}/{SourceEvent}IntegrationEventHandler.cs`
  - **Dependencies:** Outbound integration event from source module must exist

---

### Phase 6: Infrastructure Wiring

- [ ] **Task 6.1** — Register domain events in `DomainEventTypeMappings`
  - **Modifies:** `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`

- [ ] **Task 6.2** — Register notifications in OutboxModule BiMap
  - **Modifies:** `Infrastructure/Configurations/{ModuleName}Startup.cs`

- [ ] **Task 6.3** — Register internal commands in InternalCommandsModule BiMap
  - **Modifies:** `Infrastructure/Configurations/{ModuleName}Startup.cs`
  - {If no internal commands: "Skip — no internal commands in this module."}

- [ ] **Task 6.4** — Register EventsBus subscriptions
  - **Modifies:** `Infrastructure/Configurations/EventsBus/EventsBusStartup.cs` (or `EventsBusModule.cs`)

---

### Phase 7: Database Migration

- [ ] **Task 7.1** — Create read model table migration
  - **Creates:** `src/HC.LIS/HC.LIS.Database/{ModuleName}/{timestamp}_{ModuleName}Module_AddTable{ReadModelName}.cs`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

---

### Phase 8: Integration Tests (TDD)

- [ ] **Task 8.1** — Write integration tests for `Create{Aggregate}`
  - **Skill:** `/integration-test {ModuleName} create a {aggregate}`
  - **Creates:** `Tests/IntegrationTests/{Aggregates}/{Aggregate}Tests.cs`, `Get{Aggregate}DetailsFrom{ModuleName}Probe.cs`, `{Aggregate}Factory.cs`, `{Aggregate}SampleData.cs`
  - **Tests:** `Create{Aggregate}IsSuccessful`

{Repeat for each command/workflow.}

- [ ] **Task 8.N** — Verify all integration tests pass
  - **Verify:** `dotnet test Tests/IntegrationTests/` — all tests green

---

### Phase 9: Cross-Module Changes

{List changes required in OTHER modules. If none: "No cross-module changes required."}

- [ ] **Task 9.1** — Enrich `{IntegrationEvent}` in `{SourceModule}`
  - **Modifies:** `src/HC.LIS/HC.LIS.Modules/{SourceModule}/IntegrationEvents/{IntegrationEvent}.cs`
  - **New fields:** {list fields with types}
  - **Also update:** Domain event and publish notification handler that populates these fields

---

## Summary

| Phase | Task Count | Complexity |
|---|---|---|
| Module Skeleton | {n} | Low |
| Domain (TDD) | {n} | Medium-High |
| Application — Commands | {n} | Medium |
| Application — Read Model | {n} | Medium |
| Integration Events | {n} | Medium |
| Infrastructure Wiring | {n} | Low |
| Database Migration | {n} | Low |
| Integration Tests | {n} | Medium |
| Cross-Module | {n} | Varies |
| **Total** | **{total}** | |
```

---

## Rules for task generation

1. **One commit per task** — each task is a single, commit-sized unit of work
2. **TDD ordering** — failing test task immediately precedes the implementation task (Phase 2)
3. **Dependency order** — Domain → Application → Infrastructure → Integration Tests
4. **Skill reference** — every task specifies which skill to use (`/unit-test`, `/domain`, `/integration-test`, `/create-module`) or is marked as manual
5. **File listing** — every task lists exact files to create or modify (relative to module root)
6. **Verify step** — every implementation task includes a verification command
7. **Cross-module isolation** — changes to other modules are in Phase 9, clearly separated
8. **Skip markers** — Phase 1 says "Skip" if the module already exists; Phase 6.3 says "Skip" if no internal commands
9. **Accurate counts** — the Summary table task counts and phase counts must match the actual tasks listed

## How to derive tasks from the tech spec

| Tech Spec Section | Task Phase |
|---|---|
| Section 2 (Aggregate) | Phase 2 — one test+implement pair per domain method |
| Section 3 (Application) | Phase 3 — one task per command + one task per notification |
| Section 4 (Integration Events) | Phase 5 — outbound events, publish handlers, inbound handlers |
| Section 5 (Read Model) | Phase 4 — DTO, query, handler, projector |
| Section 6 (Infrastructure) | Phase 6 — wiring tasks |
| Section 7 (Migrations) | Phase 7 — one task per migration file |
| Section 8 (Unit Tests) | Phase 2 — already covered as test tasks |
| Section 9 (Integration Tests) | Phase 8 — one task per test scenario |
| Section 10 (Open Decisions) | Prerequisites or Phase 9 if they involve other modules |
