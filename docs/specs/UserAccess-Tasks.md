# Implementation Tasks: UserAccess Module

**Tech Spec:** [docs/specs/UserAccess-TechSpec.md](./UserAccess-TechSpec.md)
**Date:** 2026-04-15

---

## Prerequisites

None — module is self-contained. No cross-module enrichment or integration event changes required.

---

## Task List

### Phase 1: Module Skeleton ✓

> **Done** — `src/HC.LIS/HC.LIS.Modules/UserAccess/` already exists with full infrastructure scaffolding.

---

### Phase 2: Domain Layer (TDD) ✓

#### User creation

- [x] **Task 2.1** — Write failing unit tests for `User.Create`
  - **Skill:** `/unit-test UserAccess create a user`
  - **Creates:** `Tests/UnitTests/Users/UserTests.cs`, `UserFactory.cs`, `UserSampleData.cs`
  - **Tests:** `CreateUserIsSuccessful`
  - **Expected:** Tests fail — `UserCreatedDomainEvent`, `User.Create()`, `UserId`, `UserStatus`, `UserRole`, and `UserEmail` do not exist yet

- [x] **Task 2.2** — Implement `User` aggregate with `Create` method
  - **Skill:** `/domain UserAccess create a user`
  - **Creates:** `Domain/Users/User.cs`, `UserId.cs`, `UserEmail.cs`, `UserRole.cs`, `UserStatus.cs`, `Events/UserCreatedDomainEvent.cs`
  - **Note:** No `Apply()`/`When()` — direct field assignments. Extends `AggregateRoot` for domain event infrastructure.
  - **Verify:** Unit tests from Task 2.1 pass

#### User activation

- [x] **Task 2.3** — Write failing unit tests for `User.Activate`
  - **Skill:** `/unit-test UserAccess activate a user`
  - **Modifies:** `Tests/UnitTests/Users/UserTests.cs`
  - **Tests:** `ActivateUserIsSuccessful`, `ActivateThrowsWhenTokenIsInvalid`, `ActivateThrowsWhenUserIsAlreadyActive`
  - **Expected:** Tests fail — `UserActivatedDomainEvent`, `CannotActivateWithInvalidTokenRule`, `CannotActivateAlreadyActiveUserRule` do not exist yet

- [x] **Task 2.4** — Implement `User.Activate` method
  - **Skill:** `/domain UserAccess activate a user`
  - **Creates:** `Events/UserActivatedDomainEvent.cs`, `Rules/CannotActivateWithInvalidTokenRule.cs`, `Rules/CannotActivateAlreadyActiveUserRule.cs`
  - **Modifies:** `Domain/Users/User.cs`
  - **Verify:** Unit tests from Task 2.3 pass

#### Role change

- [x] **Task 2.5** — Write failing unit tests for `User.ChangeRole`
  - **Skill:** `/unit-test UserAccess change a user's role`
  - **Modifies:** `Tests/UnitTests/Users/UserTests.cs`
  - **Tests:** `ChangeRoleIsSuccessful`, `ChangeRoleThrowsWhenUserIsPending`
  - **Expected:** Tests fail — `UserRoleChangedDomainEvent`, `CannotChangeRoleOfPendingUserRule` do not exist yet

- [x] **Task 2.6** — Implement `User.ChangeRole` method
  - **Skill:** `/domain UserAccess change a user's role`
  - **Creates:** `Events/UserRoleChangedDomainEvent.cs`, `Rules/CannotChangeRoleOfPendingUserRule.cs`
  - **Modifies:** `Domain/Users/User.cs`
  - **Verify:** Unit tests from Task 2.5 pass; `dotnet test Tests/UnitTests/` — all 6 unit tests green

---

### Phase 3: Application Layer — Commands & Notifications

- [x] **Task 3.1** — Implement application service interfaces
  - **Manual**
  - **Creates:** `Application/Users/IPasswordHasher.cs`, `Application/Users/IJwtTokenService.cs`, `Application/Users/IEmailService.cs`, `Application/Users/IAuditLogWriter.cs`
  - **Note:** Interfaces only — implementations in Phase 6

- [x] **Task 3.2** — Implement `CreateUserCommand` + handler
  - **Manual**
  - **Creates:** `Application/Users/CreateUser/CreateUserCommand.cs`, `Application/Users/CreateUser/CreateUserCommandHandler.cs`
  - **Pattern:** `User.Create(...)` → `_context.Users.Add(user)` → `SaveChangesAsync()`; returns `UserId`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.3** — Implement `UserCreatedNotification` + invitation email handler
  - **Manual**
  - **Creates:** `Application/Users/CreateUser/UserCreatedNotification.cs`, `Application/Users/CreateUser/UserCreatedNotificationHandler.cs`
  - **Note:** Handler calls `IEmailService.SendInvitationEmailAsync(notification.DomainEvent.Email, notification.DomainEvent.InvitationToken)`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.4** — Implement `ActivateUserCommand` + handler
  - **Manual**
  - **Creates:** `Application/Users/ActivateUser/ActivateUserCommand.cs`, `Application/Users/ActivateUser/ActivateUserCommandHandler.cs`
  - **Pattern:** Load `User` by Id via EF Core; `User.Activate(invitationToken, passwordHash, activatedAt)`; `SaveChangesAsync()`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.5** — Implement `UserActivatedNotification` handler
  - **Manual**
  - **Creates:** `Application/Users/ActivateUser/UserActivatedNotification.cs`, `Application/Users/ActivateUser/UserActivatedNotificationHandler.cs`
  - **Note:** No-op handler; notification required for OutboxModule BiMap completeness
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.6** — Implement `ChangeRoleCommand` + handler
  - **Manual**
  - **Creates:** `Application/Users/ChangeRole/ChangeRoleCommand.cs`, `Application/Users/ChangeRole/ChangeRoleCommandHandler.cs`
  - **Pattern:** Load `User` by Id; `User.ChangeRole(newRole, changedById, changedAt)`; `SaveChangesAsync()`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.7** — Implement `UserRoleChangedNotification` + audit handler
  - **Manual**
  - **Creates:** `Application/Users/ChangeRole/UserRoleChangedNotification.cs`, `Application/Users/ChangeRole/UserRoleChangedNotificationHandler.cs`
  - **Note:** Handler calls `IAuditLogWriter.WriteAsync(userId, changedById, "RoleChanged", details)` where details includes old and new role
  - **Verify:** `dotnet build` succeeds

- [x] **Task 3.8** — Implement `LoginCommand` + handler + `LoginResultDto`
  - **Manual**
  - **Creates:** `Application/Users/Login/LoginCommand.cs`, `Application/Users/Login/LoginCommandHandler.cs`, `Application/Users/Login/LoginResultDto.cs`
  - **Pattern:** Dapper SELECT user by email from `user_access.users`; verify password via `IPasswordHasher`; on success: issue JWT via `IJwtTokenService`, write audit `LoginSuccess`; on failure: write audit `LoginFailed`, throw
  - **Verify:** `dotnet build` succeeds

---

### Phase 4: Application Layer — Read Models

- [x] **Task 4.1** — Implement `GetUserDetails` query + handler + DTO
  - **Manual**
  - **Creates:** `Application/Users/GetUserDetails/UserDetailsDto.cs`, `Application/Users/GetUserDetails/GetUserDetailsQuery.cs`, `Application/Users/GetUserDetails/GetUserDetailsQueryHandler.cs`
  - **Note:** Dapper `QueryFirstOrDefaultAsync` — SELECT by `id` or `email`; no projector needed
  - **Verify:** `dotnet build` succeeds

- [x] **Task 4.2** — Implement `GetUserList` query + handler + DTO
  - **Manual**
  - **Creates:** `Application/Users/GetUserList/UserListItemDto.cs`, `Application/Users/GetUserList/GetUserListQuery.cs`, `Application/Users/GetUserList/GetUserListQueryHandler.cs`
  - **Note:** Dapper `QueryAsync` — SELECT `id, email, full_name, role, status, created_at` ordered by `created_at DESC`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 4.3** — Implement `GetAuditLog` query + handler + DTO
  - **Manual**
  - **Creates:** `Application/Users/GetAuditLog/AuditLogEntryDto.cs`, `Application/Users/GetAuditLog/GetAuditLogQuery.cs`, `Application/Users/GetAuditLog/GetAuditLogQueryHandler.cs`
  - **Note:** Dapper `QueryAsync` with optional `UserId` / date range filters; ordered by `occurred_at DESC`
  - **Verify:** `dotnet build` succeeds

---

### Phase 5: Integration Events

> **Skip** — UserAccess has no inbound or outbound integration events in this release.

---

### Phase 6: Infrastructure Wiring

- [x] **Task 6.1** — Register OutboxModule BiMap in `UserAccessStartup`
  - **Manual**
  - **Modifies:** `Infrastructure/Configurations/UserAccessStartup.cs`
  - **Adds:**
    ```csharp
    notificationsBiMap.Add("UserCreatedNotification",     typeof(UserCreatedNotification));
    notificationsBiMap.Add("UserActivatedNotification",   typeof(UserActivatedNotification));
    notificationsBiMap.Add("UserRoleChangedNotification", typeof(UserRoleChangedNotification));
    ```
  - **Verify:** `dotnet build` succeeds

- [x] **Task 6.2** — Add `DbSet<User>` + `UserEntityTypeConfiguration` to `UserAccessContext`
  - **Manual**
  - **Modifies:** `Infrastructure/UserAccessContext.cs`
  - **Creates:** `Infrastructure/Users/UserEntityTypeConfiguration.cs`
  - **Note:** Table `user_access.users`, all 12 columns; EF Core value conversions for `UserId`, `UserEmail`, `UserRole`, `UserStatus`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 6.3** — Implement `PasswordHasher` + `JwtTokenService`
  - **Manual**
  - **Creates:** `Infrastructure/Authentication/PasswordHasher.cs`, `Infrastructure/Authentication/JwtTokenService.cs`
  - **Note:** `PasswordHasher` uses `Microsoft.AspNetCore.Identity.PasswordHasher<object>`; `JwtTokenService` uses `System.IdentityModel.Tokens.Jwt` with issuer/audience/key from `IConfiguration`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 6.4** — Implement `EmailService` stub
  - **Manual**
  - **Creates:** `Infrastructure/Email/EmailService.cs`
  - **Note:** Stub implementation — logs invitation token to Serilog (`ILogger`) in dev; real SMTP provider wired when PRD open question #2 is resolved
  - **Verify:** `dotnet build` succeeds

- [x] **Task 6.5** — Implement `AuditLogWriter`
  - **Manual**
  - **Creates:** `Infrastructure/AuditLog/AuditLogWriter.cs`
  - **Note:** Dapper INSERT into `user_access.audit_log`; inject `ISqlConnectionFactory`; `.ConfigureAwait(false)` on all awaited calls
  - **Verify:** `dotnet build` succeeds

---

### Phase 7: Database Migrations

- [x] **Task 7.1** — Create schema + infrastructure table migrations
  - **Manual**
  - **Creates:**
    - `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120000_UserAccessModule_AddSchemaUserAccess.cs`
    - `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120100_UserAccessModule_AddTableInboxMessages.cs`
    - `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120200_UserAccessModule_AddTableInternalCommands.cs`
    - `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120300_UserAccessModule_AddTableOutboxMessages.cs`
  - **Pattern:** Copy from `LabAnalysis/` migrations, substitute `lab_analysis` → `user_access`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

- [x] **Task 7.2** — Create `users` table migration
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120400_UserAccessModule_AddTableUsers.cs`
  - **Columns:** `id UUID PK`, `email VARCHAR(255) UNIQUE NOT NULL`, `full_name VARCHAR(255) NOT NULL`, `birthdate DATE NOT NULL`, `gender VARCHAR(20) NOT NULL`, `role VARCHAR(50) NOT NULL`, `status VARCHAR(50) NOT NULL`, `password_hash VARCHAR(500) NULL`, `invitation_token VARCHAR(100) NULL`, `created_at TIMESTAMPTZ NOT NULL`, `created_by_id UUID NULL`, `activated_at TIMESTAMPTZ NULL`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

- [x] **Task 7.3** — Create `audit_log` table migration
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120500_UserAccessModule_AddTableAuditLog.cs`
  - **Columns:** `id UUID PK`, `occurred_at TIMESTAMPTZ NOT NULL`, `user_id UUID NULL`, `actor_id UUID NULL`, `event_type VARCHAR(50) NOT NULL`, `details TEXT NULL`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds

- [x] **Task 7.4** — Create root user seed migration
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Database/UserAccess/20260415120600_UserAccessModule_SeedRootUser.cs`
  - **Note:** INSERT with id = well-known Guid, email = `root@hclis.local`, role = `ITAdmin`, status = `Active`, bcrypt hash of known dev password; production override via `ASPNETCORE_HCLIS_ROOT_PASSWORD_HASH`
  - **Verify:** `dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj` succeeds; root user row present in DB

---

### Phase 8: Integration Tests (TDD)

- [x] **Task 8.1** — Write integration tests for `CreateUser`
  - **Skill:** `/integration-test UserAccess create a user`
  - **Creates:** `Tests/IntegrationTests/Users/UserTests.cs`, `GetUserFromUserAccessProbe.cs`, `UserFactory.cs`, `UserSampleData.cs`
  - **Tests:** `CreateUserIsSuccessful` — asserts row in `user_access.users` with Status = `"PendingActivation"`

- [x] **Task 8.2** — Write integration tests for `ActivateUser`
  - **Skill:** `/integration-test UserAccess activate a user`
  - **Modifies:** `Tests/IntegrationTests/Users/UserTests.cs`
  - **Tests:** `ActivateUserIsSuccessful` — asserts Status = `"Active"`, `activated_at` set, `invitation_token` = null

- [x] **Task 8.3** — Write integration tests for `ChangeRole`
  - **Skill:** `/integration-test UserAccess change a user's role`
  - **Modifies:** `Tests/IntegrationTests/Users/UserTests.cs`
  - **Tests:** `ChangeRoleIsSuccessful` — asserts `role` column updated

- [x] **Task 8.4** — Write integration tests for `Login`
  - **Skill:** `/integration-test UserAccess login a user`
  - **Modifies:** `Tests/IntegrationTests/Users/UserTests.cs`
  - **Tests:** `LoginIsSuccessful` — asserts `LoginResultDto.Token` non-empty; `LoginFailedWritesAuditEntry` — asserts `audit_log` row with EventType = `"LoginFailed"`

- [x] **Task 8.5** — Verify all integration tests pass
  - **Manual**
  - **Verify:** `dotnet test src/HC.LIS/HC.LIS.Modules/UserAccess/Tests/IntegrationTests/HC.LIS.Modules.UserAccess.IntegrationTests.csproj` — all tests green

---

### Phase 9: Cross-Module Changes

No cross-module changes required — UserAccess is fully self-contained in this release.

---

## Summary

| Phase | Task Count | Complexity |
|---|---|---|
| Module Skeleton | 0 (skipped — already exists) | — |
| Domain (TDD) | 6 | Medium |
| Application — Commands & Notifications | 8 | Medium |
| Application — Read Models | 3 | Low |
| Integration Events | 0 (skipped — none) | — |
| Infrastructure Wiring | 5 | Low–Medium |
| Database Migration | 4 | Low |
| Integration Tests (TDD) | 5 | Medium |
| Cross-Module | 0 (none) | — |
| **Total** | **31** | |
