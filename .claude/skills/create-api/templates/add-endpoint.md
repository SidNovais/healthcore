# Template: Add Endpoint (Minimal API)

This template guides Claude through adding a single versioned REST endpoint to an existing
Minimal API endpoint group.

---

## Step 1 — Understand the request

Parse the user's description. Extract:
- **Resource** — what entity is being acted on (e.g., "order", "sample")
- **Action** — what the operation does (e.g., "create", "accept", "get by id", "list")
- **Data** — what inputs are needed (body fields, route params, query params)
- **Output** — what the caller gets back (a resource ID, a DTO, or nothing)
- **Version** — which API version this endpoint belongs to (default: v1)

---

## Step 2 — Propose a RESTful design

Before writing any code, present a concise proposal and ask the user to confirm:

```
Proposed endpoint (v1):
  Method:  POST
  Route:   /api/v1/orders/{id}/acceptance
  Request: (no body)
  Returns: 204 No Content
  Swagger status codes: 204, 401, 404, 409
  TypedResults return type: Results<NoContent, NotFound, Conflict>
```

Apply these rules:

| Action type | Method | Route pattern | Success code | TypedResults type |
|---|---|---|---|---|
| Create a resource | POST | `/api/v1/{resources}` | 201 Created + Location | `Created<Guid>` |
| Get one resource | GET | `/api/v1/{resources}/{id}` | 200 OK | `Results<Ok<Dto>, NotFound>` |
| List / search | GET | `/api/v1/{resources}?filter=…` | 200 OK | `Ok<IReadOnlyCollection<Dto>>` |
| Full update | PUT | `/api/v1/{resources}/{id}` | 204 No Content | `Results<NoContent, NotFound>` |
| Partial update | PATCH | `/api/v1/{resources}/{id}` | 204 No Content | `Results<NoContent, NotFound>` |
| Delete | DELETE | `/api/v1/{resources}/{id}` | 204 No Content | `Results<NoContent, NotFound>` |
| State transition | POST | `/api/v1/{resources}/{id}/{noun}` | 204 No Content | `Results<NoContent, NotFound, Conflict>` |

**State transition noun examples:**
- Accept → `acceptance`, Cancel → `cancellation`, Complete → `completion`, Hold → `hold`, Reject → `rejection`

Wait for the user to confirm (or correct) the design before generating code.

---

## Step 3 — Read existing files

Read:
1. `{ResourcePlural}Endpoints.cs` for the target module + resource (for the existing route list and usings)
2. `Program.cs` to confirm which version groups are mapped

---

## Step 4 — Generate three new files

Adding one endpoint always produces **up to three files** in a new action subfolder.
Never add handler code to `{ResourcePlural}Endpoints.cs` — that file is route wiring only.

### File A — `{Action}Endpoint.cs` (always required)

**Path:** `src/HC.LIS/HC.LIS.API/Modules/{ModuleName}/{ResourcePlural}/{Action}/{Action}Endpoint.cs`

Pick the pattern that matches the proposed HTTP method and return:

```csharp
// 201 Created (POST create)
using HC.Core.Application;
using HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.Create{Resource};
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Application.{ResourcePlural}.Create{Resource};
using Microsoft.AspNetCore.Http.HttpResults;

namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.Create{Resource};

internal static class Create{Resource}Endpoint
{
    internal static async Task<Created<Guid>> Handle(
        Create{Resource}Request request,
        {IModuleInterface} module,
        IExecutionContextAccessor executionContext)
    {
        var id = await module.ExecuteCommandAsync(
            new Create{Resource}Command(executionContext.UserId, request.{Property}));

        return TypedResults.Created($"/api/v1/{resource-plural-kebab}/{id}", id);
    }
}
```

```csharp
// 204 No Content (POST state transition / DELETE)
internal static class {Action}{Resource}Endpoint
{
    internal static async Task<Results<NoContent, NotFound, Conflict>> Handle(
        Guid id,
        {IModuleInterface} module,
        IExecutionContextAccessor executionContext)
    {
        await module.ExecuteCommandAsync(
            new {Action}{Resource}Command(id, executionContext.UserId));

        return TypedResults.NoContent();
    }
}
```

```csharp
// 200 OK single (GET)
internal static class Get{Resource}Endpoint
{
    internal static async Task<Results<Ok<{Resource}Dto>, NotFound>> Handle(
        Guid id,
        {IModuleInterface} module) =>
        await module.ExecuteQueryAsync(new Get{Resource}Query(id)) is { } dto
            ? TypedResults.Ok(dto)
            : TypedResults.NotFound();
}
```

```csharp
// 200 OK list (GET)
internal static class List{Resources}Endpoint
{
    internal static async Task<Ok<IReadOnlyCollection<{Resource}Dto>>> Handle(
        {IModuleInterface} module,
        string? filter = null) =>
        TypedResults.Ok(await module.ExecuteQueryAsync(new List{Resources}Query(filter)));
}
```

### File B — `{Action}Request.cs` (POST / PUT / PATCH only)

**Path:** `src/HC.LIS/HC.LIS.API/Modules/{ModuleName}/{ResourcePlural}/{Action}/{Action}Request.cs`

```csharp
namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.{Action};

/// <summary>Request body for {action description}.</summary>
public sealed record {Action}Request(
    {Type} {PropertyName}
    // one parameter per body field
);
```

Prefer `record` for request bodies. Zero logic, zero validation attributes.

### File C — Register the route in `{ResourcePlural}Endpoints.cs`

**Edit** (not create) the existing `{ResourcePlural}Endpoints.cs` — add one `MapXxx` call and
one `using` for the new action namespace:

```csharp
// add using at top:
using HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.{Action};

// add inside Map{ResourcePlural}Endpoints(), after existing registrations:

// 201 Created
group.MapPost("", Create{Resource}Endpoint.Handle)
    .WithName("Create{Resource}")
    .WithSummary("Create a new {resource}.")
    .Produces<Guid>(201)
    .ProducesProblem(400)
    .ProducesProblem(401);

// 204 state transition
group.MapPost("{id:guid}/{noun}", {Action}{Resource}Endpoint.Handle)
    .WithName("{Action}{Resource}")
    .WithSummary("{Action description}.")
    .Produces(204)
    .ProducesProblem(401)
    .ProducesProblem(404)
    .ProducesProblem(409);

// 200 single
group.MapGet("{id:guid}", Get{Resource}Endpoint.Handle)
    .WithName("Get{Resource}")
    .WithSummary("Get a {resource} by ID.")
    .Produces<{Resource}Dto>()
    .ProducesProblem(401)
    .ProducesProblem(404);

// 200 list
group.MapGet("", List{Resources}Endpoint.Handle)
    .WithName("List{Resources}")
    .WithSummary("List {resources} with optional filtering.")
    .Produces<IReadOnlyCollection<{Resource}Dto>>()
    .ProducesProblem(401);
```

---

## Step 6 — Create Command/Query stub in the module

**Command stub:**
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/{ResourcePlural}/{Action}/{Action}{Resource}Command.cs`

```csharp
using HC.LIS.Modules.{ModuleName}.Application.Contracts;

namespace HC.LIS.Modules.{ModuleName}.Application.{ResourcePlural}.{Action};

// TODO: implement handler — {Action}{Resource}CommandHandler
// Follow the existing command pattern in this module (e.g., AcceptExamCommand)
public sealed class {Action}{Resource}Command : CommandBase
{
    [method: JsonConstructor]
    public {Action}{Resource}Command({Type} {param}) => {Property} = {param};

    public {Type} {Property} { get; }
}
```

**Query stub:**
`src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/{ResourcePlural}/{Action}/{Action}{Resource}Query.cs`

```csharp
using HC.LIS.Modules.{ModuleName}.Application.Contracts;

namespace HC.LIS.Modules.{ModuleName}.Application.{ResourcePlural}.{Action};

// TODO: implement handler — {Action}{Resource}QueryHandler
public sealed class {Action}{Resource}Query : QueryBase<{ReturnDto}>
{
    public {Action}{Resource}Query({Type} {param}) => {Property} = {param};

    public {Type} {Property} { get; }
}
```

---

## Step 7 — Build verification

```bash
dotnet build src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj
```

Fix any compile errors. Report:
- Files created or modified
- Full endpoint URL (e.g., `POST /api/v1/orders/{id}/acceptance`)
- TODOs remaining (command/query handler stubs)
