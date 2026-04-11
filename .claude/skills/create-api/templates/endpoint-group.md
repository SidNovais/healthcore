# Template: Endpoint group — split-file structure

Each resource under a module produces **one folder with strictly separated files**:

```
Modules/{ModuleName}/{ResourcePlural}/
├── {ResourcePlural}Endpoints.cs     ← route wiring ONLY
└── {Action}/                        ← one subfolder per endpoint
    ├── {Action}Request.cs           ← DTO (POST/PUT/PATCH only)
    └── {Action}Endpoint.cs         ← handler only
```

---

## File 1 — `{ResourcePlural}Endpoints.cs` (route wiring only)

**Output path:** `src/HC.LIS/HC.LIS.API/Modules/{ModuleName}/{ResourcePlural}/{ResourcePlural}Endpoints.cs`

Contains **only** `MapGet/MapPost/…` registrations that delegate to handler classes.
Zero handler logic here.

```csharp
using HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.Get{Resource};
// one using per action subfolder

namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural};

internal static class {ResourcePlural}Endpoints
{
    internal static RouteGroupBuilder Map{ResourcePlural}Endpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("{ResourcePlural}");

        // TODO: add endpoint registrations using /create-api add
        // Example:
        // group.MapGet("{id:guid}", Get{Resource}Endpoint.Handle)
        //     .WithName("Get{Resource}")
        //     .WithSummary("Get a {resource} by ID.")
        //     .Produces<{Resource}Dto>()
        //     .ProducesProblem(401)
        //     .ProducesProblem(404);

        return group;
    }
}
```

---

## File 2 — `{Action}/{Action}Endpoint.cs` (handler only)

**Output path:** `src/HC.LIS/HC.LIS.API/Modules/{ModuleName}/{ResourcePlural}/{Action}/{Action}Endpoint.cs`

Contains **only** the `Handle` static method. No route registration.

### GET single resource
```csharp
using HC.Core.Application;
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Application.{ResourcePlural}.Get{Resource};
using Microsoft.AspNetCore.Http.HttpResults;

namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.Get{Resource};

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

### GET list
```csharp
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Application.{ResourcePlural}.List{Resources};
using Microsoft.AspNetCore.Http.HttpResults;

namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.List{Resources};

internal static class List{Resources}Endpoint
{
    internal static async Task<Ok<IReadOnlyCollection<{Resource}Dto>>> Handle(
        {IModuleInterface} module,
        string? filter = null) =>
        TypedResults.Ok(await module.ExecuteQueryAsync(new List{Resources}Query(filter)));
}
```

### POST create (returns 201)
```csharp
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

### POST state transition (returns 204)
```csharp
using HC.Core.Application;
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Application.{ResourcePlural}.{Action}{Resource};
using Microsoft.AspNetCore.Http.HttpResults;

namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.{Action}{Resource};

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

---

## File 3 — `{Action}/{Action}Request.cs` (DTO only, POST/PUT/PATCH)

**Output path:** `src/HC.LIS/HC.LIS.API/Modules/{ModuleName}/{ResourcePlural}/{Action}/{Action}Request.cs`

Contains **only** the DTO. No logic, no validation attributes.

```csharp
namespace HC.LIS.API.Modules.{ModuleName}.{ResourcePlural}.{Action};

/// <summary>Request body for {action description}.</summary>
public sealed record {Action}Request(
    {Type} {PropertyName}
    // one parameter per body field
);
```

Prefer `record` for immutable request bodies. Use `sealed class` with `required … { get; init; }`
only when the record syntax is awkward (e.g., many optional fields).

---

## Route registration line (in `{ResourcePlural}Endpoints.cs`)

| Pattern | Registration |
|---|---|
| GET single | `group.MapGet("{id:guid}", Get{Resource}Endpoint.Handle).WithName("Get{Resource}").WithSummary("…").Produces<{Resource}Dto>().ProducesProblem(401).ProducesProblem(404);` |
| GET list | `group.MapGet("", List{Resources}Endpoint.Handle).WithName("List{Resources}").WithSummary("…").Produces<IReadOnlyCollection<{Resource}Dto>>().ProducesProblem(401);` |
| POST create | `group.MapPost("", Create{Resource}Endpoint.Handle).WithName("Create{Resource}").WithSummary("…").Produces<Guid>(201).ProducesProblem(400).ProducesProblem(401);` |
| POST transition | `group.MapPost("{id:guid}/{noun}", {Action}{Resource}Endpoint.Handle).WithName("{Action}{Resource}").WithSummary("…").Produces(204).ProducesProblem(401).ProducesProblem(404).ProducesProblem(409);` |
| PUT update | `group.MapPut("{id:guid}", Update{Resource}Endpoint.Handle).WithName("Update{Resource}").WithSummary("…").Produces(204).ProducesProblem(400).ProducesProblem(401).ProducesProblem(404);` |
| DELETE | `group.MapDelete("{id:guid}", Delete{Resource}Endpoint.Handle).WithName("Delete{Resource}").WithSummary("…").Produces(204).ProducesProblem(401).ProducesProblem(404);` |
