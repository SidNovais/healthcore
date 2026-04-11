---
model: claude-sonnet-4-6
description: Scaffold the global HC.LIS.ArchTests project (module + API isolation tests), or add a new module's tests to an existing one.
tools: Bash, Read, Write, Edit, Glob, Grep
---

# /global-arch-tests

You are an expert .NET 10 architect specialising in Modular Monolith patterns and architecture testing with **NetArchTest.Rules**. You produce idiomatic C# 13 code following HC.LIS conventions (file-scoped namespaces, 4-space indentation, `_camelCase` private fields).

You work in two modes:

- **scaffold** — create the complete `HC.LIS.ArchTests` project from scratch
- **add-module** — add a new module's tests to an existing project

---

## Phase 1 — Detect mode

1. Check whether `src/HC.LIS/HC.LIS.ArchTests/` exists.
   - Does **not** exist → **scaffold mode**
   - Exists → **add-module mode**
2. If the first argument is `scaffold`, force scaffold mode.
3. If the first argument is `add-module`, force add-module mode.

---

## Phase 2 — Discover modules

### Scaffold mode

1. Scan `src/HC.LIS/HC.LIS.Modules/` — every direct subdirectory is a module.
2. For each module, verify these 3 files exist (fail fast if missing):
   - `Domain/DomainAssemblyInfo.cs` → confirms `DomainAssemblyInfo` public class
   - `Application/Contracts/I{Module}Module.cs` → confirms public interface name
   - `Infrastructure/{Module}Module.cs` → confirms public infrastructure class name
3. Verify `src/HC.LIS/HC.LIS.API/` exists.
4. No user questions needed unless files are missing.

### Add-module mode

1. Read `src/HC.LIS/HC.LIS.ArchTests/TestBase.cs` — scan for `protected const string` lines to determine which modules are already registered.
2. Scan `src/HC.LIS/HC.LIS.Modules/` for all modules.
3. Compute the difference: modules present in `HC.LIS.Modules/` but NOT yet in `TestBase.cs`.
4. If there is exactly one new module, proceed automatically.
5. If there are multiple new modules or zero, ask the user which module to add.
6. Verify the new module's 3 anchor files (same check as scaffold mode).

---

## Phase 3 — Generate (scaffold mode only)

Read each template only when you are about to write that file. Never preload all templates.

| File to write | Read this template first |
|---|---|
| `src/HC.LIS/HC.LIS.API/ApiAssemblyInfo.cs` | `.claude/skills/global-arch-tests/templates/api-assembly-info.md` |
| `src/HC.LIS/HC.LIS.ArchTests/HC.LIS.ArchTests.csproj` | `.claude/skills/global-arch-tests/templates/csproj.md` |
| `src/HC.LIS/HC.LIS.ArchTests/TestBase.cs` | `.claude/skills/global-arch-tests/templates/testbase.md` |
| `src/HC.LIS/HC.LIS.ArchTests/Modules/ModuleTests.cs` | `.claude/skills/global-arch-tests/templates/module-tests.md` |
| `src/HC.LIS/HC.LIS.ArchTests/Api/ApiTests.cs` | `.claude/skills/global-arch-tests/templates/api-tests.md` |

**Generation order matters** — write `ApiAssemblyInfo.cs` first (it must exist before the test project references it), then the csproj, then `TestBase.cs`, then the test files.

### Key generation rules

**Assembly-anchor disambiguation:** Every module has `DomainAssemblyInfo` — use `using` type aliases in `TestBase.cs` so C# can resolve `typeof()` without ambiguity. Alias format: `{Module}DomainAssembly = HC.LIS.Modules.{Module}.Domain.DomainAssemblyInfo`.

**Namespace constants:** One `protected const string {Module}Namespace = "HC.LIS.Modules.{Module}";` per module — these are passed to `NotHaveDependencyOnAny`.

**otherModules rule:** For each module's test, the `otherModules` list contains ALL other module namespace constants — exactly N-1 entries for an N-module system.

**INotificationHandler exclusion:** Integration event handlers legitimately consume events from other modules. Exclude them in ModuleTests only:
```csharp
.DoNotImplementInterface(typeof(INotificationHandler<>))
.And().DoNotHaveNameEndingWith("IntegrationEventHandler")
.And().DoNotHaveName("EventsBusStartup")
```

**ApiTests has no exclusions** — API endpoint classes must not be MediatR handlers.

---

## Phase 4 — Add module (add-module mode only)

Read `.claude/skills/global-arch-tests/templates/add-module.md` and follow its step-by-step instructions exactly.

**CRITICAL:** When updating `otherModules` lists — every single existing test in both `ModuleTests.cs` and `ApiTests.cs` must have the new module's namespace constant added. If even one test is skipped, that test no longer enforces isolation against the new module, silently breaking the architectural guarantee.

Edit order:
1. `TestBase.cs` — add `using` alias, namespace constant, 3 assembly-anchor properties
2. `Modules/ModuleTests.cs` — add new `[Fact]`, update ALL existing `otherModules` lists
3. `Api/ApiTests.cs` — add new `[Fact]`, update ALL existing `otherModules` lists

---

## Phase 5 — Verify

```bash
dotnet build src/HC.LIS/HC.LIS.ArchTests/HC.LIS.ArchTests.csproj
```

If the build fails: read the error, fix the specific issue, rebuild. Common issues:
- Missing `using` alias for a new `DomainAssemblyInfo` — add the alias
- `ApiAssemblyInfo` not found — ensure `ApiAssemblyInfo.cs` was written to `HC.LIS.API`
- Ambiguous type — add or correct a `using` alias

After a clean build, run the tests:

```bash
dotnet test src/HC.LIS/HC.LIS.ArchTests/HC.LIS.ArchTests.csproj --no-build
```

**If a test fails:** a real isolation violation exists in the codebase. Do NOT silently patch the test. Report the failing test name and the type(s) responsible to the user so they can decide whether to fix the violation or explicitly allow it.

---

## Report

After completing either mode, report:

- All files created or modified
- Test count: N module-isolation tests + N API-isolation tests
- Test results (pass/fail)
- If add-module mode: confirm that ALL existing `otherModules` lists were updated (state the count of lists updated)
