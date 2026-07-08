using System.Globalization;
using Asp.Versioning;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HC.Core.Application;
using HC.LIS.API.Configuration.Authentication;
using HC.LIS.API.Configuration.ExecutionContext;
using HC.LIS.API.Configuration.Extensions;
using HC.LIS.API.Configuration.Validation;
using HC.LIS.API.Modules.Analyzer;
using HC.LIS.API.Modules.Analyzer.AnalyzerSamples;
using HC.LIS.API.Modules.LabAnalysis;
using HC.LIS.API.Modules.LabAnalysis.WorklistItems;
using HC.LIS.API.Modules.SampleCollection;
using HC.LIS.API.Modules.SampleCollection.CollectionRequests;
using HC.LIS.API.Modules.SampleCollection.Samples;
using HC.LIS.API.Modules.TestOrders;
using HC.LIS.API.Modules.TestOrders.Orders;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations;
using HC.LIS.API.Modules.UserAccess;
using HC.LIS.API.Modules.UserAccess.Auth;
using HC.LIS.API.Modules.UserAccess.AuditLog;
using HC.LIS.API.Modules.UserAccess.Users;
using HC.LIS.Modules.UserAccess.Infrastructure.Configurations;
using HC.LIS.API.Configuration.EventBus;
using HC.LIS.API.Modules.PatientManagement;
using HC.LIS.API.Modules.PatientManagement.Patients;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

// ─── Logger ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] [{TraceId}] {Message:lj}{NewLine}{Exception}",
        formatProvider: CultureInfo.InvariantCulture)
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
              .Enrich.WithSpan()
              .WriteTo.Console(
                  outputTemplate:
                      "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] [{TraceId}] {Message:lj}{NewLine}{Exception}",
                  formatProvider: CultureInfo.InvariantCulture));

    // ─── Autofac ───────────────────────────────────────────────────────────
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new TestOrdersAutofacModule());
        containerBuilder.RegisterModule(new SampleCollectionAutofacModule());
        containerBuilder.RegisterModule(new AnalyzerAutofacModule());
        containerBuilder.RegisterModule(new LabAnalysisAutofacModule());
        containerBuilder.RegisterModule(new UserAccessAutofacModule());
        containerBuilder.RegisterModule(new PatientManagementAutofacModule());
    });

    // ─── OpenTelemetry ─────────────────────────────────────────────────────
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("HC.LIS.API"))
        .WithTracing(tracing => tracing
            .AddSource(
                "HC.LIS.TestOrders",
                "HC.LIS.Analyzer",
                "HC.LIS.LabAnalysis",
                "HC.LIS.SampleCollection",
                "HC.LIS.PatientManagement",
                "HC.LIS.UserAccess")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddQuartzInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter())
        .WithMetrics(metrics => metrics
            .AddMeter("HC.LIS")
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter());

    // ─── Services ──────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

    builder.Services.AddHcLisJwtCookieAuthentication(builder.Configuration);
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ITAdmin", policy => policy.RequireRole("ITAdmin"));
        options.AddPolicy("PatientManagement", policy => policy.RequireRole("Receptionist", "ITAdmin"));
    });

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddCors(options =>
            options.AddPolicy("DevCors", policy =>
                policy.SetIsOriginAllowed(_ => true)
                      .AllowCredentials()
                      .AllowAnyMethod()
                      .AllowAnyHeader()));
    }

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

    using var busProvider = await ModuleEventBusFactoryBuilder
        .CreateAsync(builder.Configuration, Log.Logger).ConfigureAwait(false);

    TestOrdersStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: busProvider.TestOrders);
    SampleCollectionStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: busProvider.SampleCollection);
    AnalyzerStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: busProvider.Analyzer);
    LabAnalysisStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: busProvider.LabAnalysis);
    UserAccessStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: busProvider.UserAccess);
    PatientManagementStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: busProvider.PatientManagement);

    busProvider.StartConsuming();

    // ─── Middleware pipeline ────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
        app.UseCors("DevCors");

    app.UseHcLisExceptionHandler();
    app.UseSwaggerDocumentation();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // ─── Versioned endpoint groups ──────────────────────────────────────────
    var versionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    var v1 = app.MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(versionSet)
        .MapToApiVersion(1)
        .RequireAuthorization();

    var v1Anon = app.MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(versionSet)
        .MapToApiVersion(1);

    v1Anon.MapGroup("auth").MapAuthEndpoints();

    v1.MapGroup("orders").MapOrdersEndpoints();
    v1.MapGroup("samples").MapSamplesEndpoints();
    v1.MapGroup("collection-requests").MapCollectionRequestsEndpoints();
    v1.MapGroup("analyzer-samples").MapAnalyzerSamplesEndpoints();
    v1.MapGroup("worklist-items").MapWorklistItemsEndpoints();
    v1.MapGroup("users").MapUsersEndpoints();
    v1.MapGroup("audit-log").MapAuditLogEndpoints();
    v1.MapGroup("patients").MapPatientsEndpoints();

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "HC.LIS.API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
