# Infrastructure wiring section template

## When to use

Read this template when generating **Section 6 (Infrastructure Wiring)** of the tech spec.

---

## Section format

```markdown
## 6. Infrastructure Wiring

### 6.1 DomainEventTypeMappings

Register all {N} domain events in `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`:

{code block with registrations}

### 6.2 {ModuleName}Startup — OutboxModule BiMap

Register all {N} notification type mappings in `Infrastructure/Configurations/{ModuleName}Startup.cs`:

{code block with BiMap entries}

### 6.3 {ModuleName}Startup — InternalCommandsModule BiMap

{If any internal commands exist, list them. Otherwise: "No internal commands in this module."}

### 6.4 EventsBus Subscription

Register `{HandlerName}` in the `EventsBusModule` to subscribe to `{IntegrationEvent}`.

### 6.5 Module Facade

`I{ModuleName}Module` and `{ModuleName}Module` {already scaffolded / needs changes} — generic dispatcher pattern.
```

---

## Conventions

### DomainEventTypeMappings

- File: `Infrastructure/Configurations/AggregateStore/DomainEventTypeMappings.cs`
- One line per domain event:
  ```csharp
  options.Events.AddEventType<{DomainEvent}>();
  ```
- Register ALL domain events from Section 2.5 — missing registrations cause runtime deserialization failures

### OutboxModule BiMap

- Location: `Infrastructure/Configurations/{ModuleName}Startup.cs` inside the `Initialize()` method
- One entry per notification from Section 3.2:
  ```csharp
  notificationsBiMap.Add("{NotificationName}", typeof({NotificationClass}));
  ```
- The string key is the notification class name (without namespace)
- **Startup validates at boot** that every domain event notification has a BiMap entry — missing entries crash the app

### InternalCommandsModule BiMap

- Same pattern as OutboxModule but for `InternalCommandBase` subclasses
- Only needed when the module schedules async commands via Quartz
- Entry format:
  ```csharp
  internalCommandsBiMap.Add("{CommandName}", typeof({CommandClass}));
  ```

### EventsBus subscription

- Register inbound integration event handlers so the Inbox job can dispatch them
- One subscription per inbound integration event from Section 4.1

### Module facade

- `I{ModuleName}Module` interface with `ExecuteCommandAsync<T>()` and `ExecuteQueryAsync<T>()`
- `{ModuleName}Module` implementation delegates to `CommandsExecutor` and `QueryExecutor`
- Usually scaffolded by `/create-module` — note if it needs changes

---

## Example (from LabAnalysis-TechSpec.md)

```markdown
### 6.1 DomainEventTypeMappings

options.Events.AddEventType<WorklistItemCreatedDomainEvent>();
options.Events.AddEventType<AnalysisResultRecordedDomainEvent>();
options.Events.AddEventType<ReportGeneratedDomainEvent>();
options.Events.AddEventType<WorklistItemCompletedDomainEvent>();

### 6.2 LabAnalysisStartup — OutboxModule BiMap

notificationsBiMap.Add("WorklistItemCreatedNotification",       typeof(WorklistItemCreatedNotification));
notificationsBiMap.Add("AnalysisResultRecordedNotification",    typeof(AnalysisResultRecordedNotification));
notificationsBiMap.Add("ReportGeneratedNotification",           typeof(ReportGeneratedNotification));
notificationsBiMap.Add("WorklistItemCompletedNotification",     typeof(WorklistItemCompletedNotification));

### 6.3 EventsBus Subscription

Register `SampleCollectedIntegrationEventHandler` in the `EventsBusModule` to subscribe to `SampleCollectedIntegrationEvent`.

### 6.4 Module Facade

`ILabAnalysisModule` and `LabAnalysisModule` already scaffolded — generic dispatcher pattern, no changes needed.
```

---

## Checklist before moving to Section 7

- [ ] Every domain event from Section 2.5 is registered in DomainEventTypeMappings
- [ ] Every notification from Section 3.2 is registered in the OutboxModule BiMap
- [ ] Every inbound integration event from Section 4.1 has an EventsBus subscription
- [ ] Internal commands BiMap is present (even if empty / "N/A")
- [ ] Module facade status is documented (scaffolded vs. needs changes)
- [ ] Count of registrations matches (N events = N DomainEventTypeMappings = N BiMap entries)
