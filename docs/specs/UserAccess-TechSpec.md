# Technical Spec: UserAccess Module

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-04-15
**PRD Reference:** [docs/prd/UserAccess.md](../prd/UserAccess.md)

---

## 1. Overview

The UserAccess module provides authentication and role-based authorization for the HC.LIS platform. It manages the user lifecycle — from admin provisioning and invitation-based first-password setup, through JWT-based login and role management. Every login attempt and role change is recorded in an audit log for HIPAA compliance.

**Aggregate root:** `User`
**Schema:** `user_access`
**Storage:** EF Core (relational) — **no Marten event store for this module.** `User` is stored in a single `user_access.users` table. No `Apply()`/`When()` dispatch — command methods use direct field assignments.

---

## 2. Aggregate: `User`

### 2.1 Identity

`UserId` — typed ID wrapping `Guid`.

EF Core entity mapped to `user_access.users`. Not event-sourced. EF Core change tracking handles persistence after domain method calls.

### 2.2 State Machine

```
PendingActivation → Active
```

| Status | Meaning |
|---|---|
| `PendingActivation` | User created by admin; invitation email sent; password not yet set |
| `Active` | User has set their initial password; can log in |

### 2.3 Domain Methods & Events

| Method | Business Rule(s) | Domain Event Emitted |
|---|---|---|
| `Create(Guid userId, string email, string fullName, DateTime birthdate, string gender, string role, string invitationToken, DateTime createdAt, Guid createdById)` | — | `UserCreatedDomainEvent` |
| `Activate(string invitationToken, string passwordHash, DateTime activatedAt)` | `CannotActivateWithInvalidTokenRule`, `CannotActivateAlreadyActiveUserRule` | `UserActivatedDomainEvent` |
| `ChangeRole(string newRole, Guid changedById, DateTime changedAt)` | `CannotChangeRoleOfPendingUserRule` | `UserRoleChangedDomainEvent` |

### 2.4 Business Rules

| Class | Invariant |
|---|---|
| `CannotActivateWithInvalidTokenRule` | The provided invitation token must match the token stored on the user |
| `CannotActivateAlreadyActiveUserRule` | Status must be `PendingActivation` to activate |
| `CannotChangeRoleOfPendingUserRule` | Status must be `Active` to change a user's role |

### 2.5 Domain Events (fields)

**`UserCreatedDomainEvent`**
- `UserId` (Guid), `Email` (string), `FullName` (string), `Birthdate` (DateTime), `Gender` (string), `Role` (string), `InvitationToken` (string), `CreatedAt` (DateTime), `CreatedById` (Guid)

**`UserActivatedDomainEvent`**
- `UserId` (Guid), `ActivatedAt` (DateTime)

**`UserRoleChangedDomainEvent`**
- `UserId` (Guid), `OldRole` (string), `NewRole` (string), `ChangedById` (Guid), `ChangedAt` (DateTime)

---

## 3. Application Layer

### 3.1 Commands

Location: `Application/Users/{CommandName}/`

| Command | Properties | Action |
|---|---|---|
| `CreateUserCommand` | `UserId (Guid)`, `Email (string)`, `FullName (string)`, `Birthdate (DateTime)`, `Gender (string)`, `Role (string)`, `InvitationToken (string)`, `CreatedAt (DateTime)`, `CreatedById (Guid)` — extends `CommandBase<Guid>` | `User.Create(...)` → `_context.Users.Add(user)` → `SaveChangesAsync()` |
| `ActivateUserCommand` | `UserId (Guid)`, `InvitationToken (string)`, `PasswordHash (string)`, `ActivatedAt (DateTime)` — extends `CommandBase` | Load `User` by Id from EF Core; `User.Activate(...)` → `SaveChangesAsync()` |
| `ChangeRoleCommand` | `UserId (Guid)`, `NewRole (string)`, `ChangedById (Guid)`, `ChangedAt (DateTime)` — extends `CommandBase` | Load `User` by Id; `User.ChangeRole(...)` → `SaveChangesAsync()` |
| `LoginCommand` | `Email (string)`, `Password (string)` — extends `CommandBase<LoginResultDto>` | Dapper SELECT user by email; verify hash via `IPasswordHasher`; issue JWT via `IJwtTokenService`; write audit via `IAuditLogWriter`; return `LoginResultDto` |

`LoginResultDto` — `{ string Token, Guid UserId, string UserEmail, string Role }`

> `LoginCommand` does not interact with the aggregate — it is a cross-cutting application service command.

### 3.2 Notifications

Domain events are dispatched by `DomainEventsDispatcher` after `SaveChangesAsync()`. One notification per domain event, co-located with the command that triggers it.

| Notification | Co-located With | Side Effects |
|---|---|---|
| `UserCreatedNotification` | `CreateUser/` | Sends invitation email via `IEmailService.SendInvitationEmailAsync(email, token)` |
| `UserActivatedNotification` | `ActivateUser/` | None — state already persisted via EF Core |
| `UserRoleChangedNotification` | `ChangeRole/` | Writes audit entry via `IAuditLogWriter` (EventType = `"RoleChanged"`) |

### 3.3 Projection Handlers

**None.** `UserAccessContext.Users` is both the write model and the read model. Domain events do not project to separate read model tables.

### 3.4 Application Service Interfaces

Defined in `Application/Users/`:

| Interface | Methods |
|---|---|
| `IPasswordHasher` | `string HashPassword(string plainText)`, `bool VerifyHashedPassword(string hash, string plainText)` |
| `IJwtTokenService` | `string GenerateToken(Guid userId, string email, string role)` |
| `IEmailService` | `Task SendInvitationEmailAsync(string toEmail, string invitationToken)` |
| `IAuditLogWriter` | `Task WriteAsync(Guid? userId, Guid? actorId, string eventType, string? details)` |

---

## 4. Integration Events

### 4.1 Inbound — Subscription

None — UserAccess does not subscribe to any other module's integration events.

### 4.2 Outbound — Integration Events

None — no other module reacts to user lifecycle events in this release.

---

## 5. Read Models

All queries use Dapper and read directly from EF Core-owned tables. No projectors or projected read model tables.

### 5.1 `users` Table (Write + Read Model)

| Column | Type | Set by |
|---|---|---|
| `id` | UUID PK | `User.Create()` |
| `email` | VARCHAR(255) UNIQUE NOT NULL | `User.Create()` |
| `full_name` | VARCHAR(255) NOT NULL | `User.Create()` |
| `birthdate` | DATE NOT NULL | `User.Create()` |
| `gender` | VARCHAR(20) NOT NULL | `User.Create()` |
| `role` | VARCHAR(50) NOT NULL | `User.Create()`, `User.ChangeRole()` |
| `status` | VARCHAR(50) NOT NULL | All state transitions |
| `password_hash` | VARCHAR(500) NULL | `User.Activate()` |
| `invitation_token` | VARCHAR(100) NULL | `User.Create()` (set to `null` on activation) |
| `created_at` | TIMESTAMPTZ NOT NULL | `User.Create()` |
| `created_by_id` | UUID NULL | `User.Create()` |
| `activated_at` | TIMESTAMPTZ NULL | `User.Activate()` |

**GetUserDetails application files** (`Application/Users/GetUserDetails/`):
- `UserDetailsDto.cs` — immutable record with all 12 columns
- `GetUserDetailsQuery.cs` — `IQuery<UserDetailsDto?>`, accepts `Guid UserId` or `string Email`
- `GetUserDetailsQueryHandler.cs` — Dapper `QueryFirstOrDefaultAsync` by Id or Email

**GetUserList application files** (`Application/Users/GetUserList/`):
- `UserListItemDto.cs` — record with `Id, Email, FullName, Role, Status, CreatedAt`
- `GetUserListQuery.cs` — `IQuery<IReadOnlyCollection<UserListItemDto>>`
- `GetUserListQueryHandler.cs` — Dapper `QueryAsync`, ordered by `created_at DESC`

### 5.2 `audit_log` Table

| Column | Type |
|---|---|
| `id` | UUID PK |
| `occurred_at` | TIMESTAMPTZ NOT NULL |
| `user_id` | UUID NULL (null when actor is unknown, e.g., failed login with unrecognized email) |
| `actor_id` | UUID NULL |
| `event_type` | VARCHAR(50) NOT NULL — `LoginSuccess`, `LoginFailed`, `RoleChanged` |
| `details` | TEXT NULL |

Written directly by `IAuditLogWriter` (Dapper INSERT). Not projected from domain events.

Written by:
- `LoginCommandHandler` — for both `LoginSuccess` and `LoginFailed` outcomes
- `UserRoleChangedNotificationHandler` — for `RoleChanged` via notification (outbox path)

**GetAuditLog application files** (`Application/Users/GetAuditLog/`):
- `AuditLogEntryDto.cs`
- `GetAuditLogQuery.cs` — `IQuery<IReadOnlyCollection<AuditLogEntryDto>>`, optional `UserId` and date range filters
- `GetAuditLogQueryHandler.cs` — Dapper `QueryAsync` ordered by `occurred_at DESC`

---

## 6. Infrastructure Wiring

### 6.1 DomainEventTypeMappings

No changes required — stays empty. `User` is an EF Core entity, not a Marten aggregate. No domain event type registration is needed.

### 6.2 UserAccessStartup — OutboxModule BiMap

Register all 3 notification type mappings in `Infrastructure/Configurations/UserAccessStartup.cs`:

```csharp
notificationsBiMap.Add("UserCreatedNotification",     typeof(UserCreatedNotification));
notificationsBiMap.Add("UserActivatedNotification",   typeof(UserActivatedNotification));
notificationsBiMap.Add("UserRoleChangedNotification", typeof(UserRoleChangedNotification));
```

### 6.3 UserAccessStartup — InternalCommandsModule BiMap

No internal commands in this module.

### 6.4 EventsBus Subscription

No subscriptions — UserAccess does not receive integration events from other modules.

### 6.5 UserAccessContext

Add `DbSet<User> Users` to `Infrastructure/UserAccessContext.cs` and register `UserEntityTypeConfiguration` in `OnModelCreating`. The configuration maps to the `user_access.users` table with all 12 columns.

### 6.6 Module Facade

`IUserAccessModule` and `UserAccessModule` already scaffolded — generic dispatcher pattern, no changes needed.

### 6.7 Infrastructure Implementations

| Class | Location | Implements |
|---|---|---|
| `PasswordHasher` | `Infrastructure/Authentication/` | `IPasswordHasher` — uses `Microsoft.AspNetCore.Identity.PasswordHasher<object>` |
| `JwtTokenService` | `Infrastructure/Authentication/` | `IJwtTokenService` — uses `System.IdentityModel.Tokens.Jwt`; reads issuer, audience, key from config |
| `EmailService` | `Infrastructure/Email/` | `IEmailService` — stubbed for this release; logs invitation token via Serilog in dev (real SMTP provider TBD) |
| `AuditLogWriter` | `Infrastructure/AuditLog/` | `IAuditLogWriter` — Dapper INSERT into `user_access.audit_log` |

---

## 7. Database Migrations

Location: `src/HC.LIS/HC.LIS.Database/UserAccess/`

| File | Purpose |
|---|---|
| `20260415120000_UserAccessModule_AddSchemaUserAccess.cs` | Create `user_access` schema |
| `20260415120100_UserAccessModule_AddTableInboxMessages.cs` | `user_access.inbox_messages` table |
| `20260415120200_UserAccessModule_AddTableInternalCommands.cs` | `user_access.internal_commands` table |
| `20260415120300_UserAccessModule_AddTableOutboxMessages.cs` | `user_access.outbox_messages` table |
| `20260415120400_UserAccessModule_AddTableUsers.cs` | `user_access.users` table — all 12 columns |
| `20260415120500_UserAccessModule_AddTableAuditLog.cs` | `user_access.audit_log` table |
| `20260415120600_UserAccessModule_SeedRootUser.cs` | INSERT root user: email `root@hclis.local`, role `ITAdmin`, status `Active`, bcrypt hash; production hash via `ASPNETCORE_HCLIS_ROOT_PASSWORD_HASH` env var |

---

## 8. Unit Tests

Location: `Tests/UnitTests/Users/UserTests.cs`
Pattern: Arrange–Act–Assert, `AssertPublishedDomainEvent<T>()` on aggregate, FluentAssertions.

| Test | Asserts |
|---|---|
| `CreateUserIsSuccessful` | `UserCreatedDomainEvent` raised with correct fields |
| `ActivateUserIsSuccessful` | `UserActivatedDomainEvent` raised |
| `ChangeRoleIsSuccessful` | `UserRoleChangedDomainEvent` raised; `OldRole` and `NewRole` fields correct |
| `ActivateThrowsWhenTokenIsInvalid` | `BaseBusinessRuleException` with `CannotActivateWithInvalidTokenRule` |
| `ActivateThrowsWhenUserIsAlreadyActive` | `BaseBusinessRuleException` with `CannotActivateAlreadyActiveUserRule` |
| `ChangeRoleThrowsWhenUserIsPending` | `BaseBusinessRuleException` with `CannotChangeRoleOfPendingUserRule` |

---

## 9. Integration Tests

Location: `Tests/IntegrationTests/Users/`
Pattern: `TestBase.ExecuteCommandAsync(command)` → direct Dapper probe against `user_access.users` or `user_access.audit_log`.

| Test | Command Sent | Assertion |
|---|---|---|
| `CreateUserIsSuccessful` | `CreateUserCommand` | Row in `user_access.users` with Status = `"PendingActivation"` and all fields set |
| `ActivateUserIsSuccessful` | `ActivateUserCommand` | Status = `"Active"`, `activated_at` set, `invitation_token` = null |
| `ChangeRoleIsSuccessful` | `ChangeRoleCommand` | `role` column updated in `user_access.users` |
| `LoginIsSuccessful` | `LoginCommand` | Returns `LoginResultDto` with non-empty `Token` |
| `LoginFailedWritesAuditEntry` | `LoginCommand` (wrong password) | Row in `user_access.audit_log` with EventType = `"LoginFailed"` |

---

## 10. Open Design Decisions

| # | Decision | Options | Recommendation |
|---|---|---|---|
| 1 | SMTP provider for invitation emails | MailKit, SendGrid, AWS SES | `IEmailService` stubbed for this release — logs invitation token via Serilog in dev. Choose real provider when PRD open question #2 is resolved. |
| 2 | JWT token expiry + refresh strategy | Short-lived access token only vs. access + refresh token pair | Short-lived access token (e.g., 1 hour) for this release; refresh token pattern deferred per PRD §7 |
| 3 | Root user password mechanism | Hardcoded dev hash in migration vs. env-var override | Hardcode a bcrypt hash of a known dev password in the migration; `ASPNETCORE_HCLIS_ROOT_PASSWORD_HASH` env var overrides in production |
| 4 | Cross-module audit logging (reads/writes in TestOrders, SampleCollection, LabAnalysis) | In scope for next release | Deferred per PRD §7 — owner: IT Admin / Lab Manager |
