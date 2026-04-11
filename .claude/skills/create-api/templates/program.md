# Template: Program.cs

**Output path:** `src/HC.LIS/HC.LIS.API/Program.cs`

This is the **single entry point** — there is no `Startup.cs`. All wiring is done here via extension
methods defined in `Configuration/` and `Modules/`.

**Placeholders** (Claude fills in based on Q&A answers):
- `__ModuleUsings__` — one `using` per selected module's Infrastructure.Configurations namespace
- `__ApiModuleUsings__` — one `using` per generated `{M}AutofacModule` namespace
- `__AutofacRegistrations__` — one `RegisterModule` per module
- `__ModuleInitializations__` — one `{M}Startup.Initialize(...)` call per module
- `__AuthServices__` — JWT services block (or empty)
- `__CorsServices__` — CORS services block (or empty)
- `__CorsMiddleware__` — CORS middleware (or empty)
- `__AuthMiddleware__` — auth + authorization middleware (or empty)
- `__V1EndpointGroups__` — one `.MapGroup("{resource}").Map{Resource}Endpoints()` per module
- `__RequireAuth__` — `.RequireAuthorization()` (or empty if no auth)

```csharp
using Asp.Versioning;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HC.Core.Application;
using HC.LIS.API.Configuration.Authentication;
using HC.LIS.API.Configuration.ExecutionContext;
using HC.LIS.API.Configuration.Extensions;
using HC.LIS.API.Configuration.Validation;
using Serilog;
using Serilog.Events;
__ModuleUsings__
__ApiModuleUsings__

// ─── Logger ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Configuration ─────────────────────────────────────────────────────
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables("ASPNETCORE_HCLIS_");

    // ─── Logging ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, _, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .Enrich.FromLogContext()
              .WriteTo.Console(
                  outputTemplate:
                      "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}"));

    // ─── Autofac ───────────────────────────────────────────────────────────
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        __AutofacRegistrations__
    });

    // ─── Services ──────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

    __AuthServices__

    __CorsServices__

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddSwaggerDocumentation(
        builder.Configuration["API_TITLE"] ?? "HC.LIS API",
        builder.Configuration["API_DESCRIPTION"] ?? "Laboratory Information System — Modular Monolith API");

    // ─── Build ─────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─── Module initialization ─────────────────────────────────────────────
    var connectionString = builder.Configuration["DATABASE_CONNECTION_STRING"]
        ?? throw new InvalidOperationException(
            "Missing required env var: ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING");

    var executionContext = app.Services.GetRequiredService<IExecutionContextAccessor>();

    __ModuleInitializations__

    // ─── Middleware pipeline ────────────────────────────────────────────────
    __CorsMiddleware__

    app.UseHcLisExceptionHandler();
    app.UseSwaggerDocumentation();
    app.UseHttpsRedirection();

    __AuthMiddleware__

    // ─── Versioned endpoint groups ──────────────────────────────────────────
    var versionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    var v1 = app.MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(versionSet)
        .MapToApiVersion(1)
        __RequireAuth__;

    __V1EndpointGroups__

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "HC.LIS.API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

---

## Dynamic block specifications

### `__AutofacRegistrations__`
```csharp
containerBuilder.RegisterModule(new TestOrdersAutofacModule());
containerBuilder.RegisterModule(new SampleCollectionAutofacModule());
// one per selected module
```

### `__ModuleInitializations__`
Standard pattern (verify exact signature from each module's `{M}Startup.cs`):
```csharp
TestOrdersStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);
SampleCollectionStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);
```

### `__AuthServices__`
**JWT Bearer:**
```csharp
builder.Services.AddHcLisJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
```
**No auth:** (omit block entirely)

### `__CorsServices__`
**Dev-only:**
```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddPolicy("DevCors", policy =>
            policy.SetIsOriginAllowed(_ => true)
                  .AllowCredentials()
                  .AllowAnyMethod()
                  .AllowAnyHeader()));
}
```

### `__CorsMiddleware__`
**Dev-only:**
```csharp
if (app.Environment.IsDevelopment())
    app.UseCors("DevCors");
```

### `__AuthMiddleware__`
**With auth:**
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### `__V1EndpointGroups__`
One `.MapGroup` call per selected module's primary resource:
```csharp
v1.MapGroup("orders").MapOrdersEndpoints();
v1.MapGroup("samples").MapSamplesEndpoints();
```
The route segment is plural noun, kebab-case (matches the endpoint group class definition).

### `__RequireAuth__`
**With auth:**
```csharp
.RequireAuthorization()
```
**No auth:** (omit)
