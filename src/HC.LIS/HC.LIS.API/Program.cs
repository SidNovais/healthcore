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
using HC.LIS.API.Modules.SampleCollection.Samples;
using HC.LIS.API.Modules.TestOrders;
using HC.LIS.API.Modules.TestOrders.Orders;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations;
using Serilog;
using Serilog.Events;

// ─── Logger ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}",
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
              .WriteTo.Console(
                  outputTemplate:
                      "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}",
                  formatProvider: CultureInfo.InvariantCulture));

    // ─── Autofac ───────────────────────────────────────────────────────────
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new TestOrdersAutofacModule());
        containerBuilder.RegisterModule(new SampleCollectionAutofacModule());
        containerBuilder.RegisterModule(new AnalyzerAutofacModule());
        containerBuilder.RegisterModule(new LabAnalysisAutofacModule());
    });

    // ─── Services ──────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

    builder.Services.AddHcLisJwtCookieAuthentication(builder.Configuration);
    builder.Services.AddAuthorization();

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

    TestOrdersStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);
    SampleCollectionStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);
    AnalyzerStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);
    LabAnalysisStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);

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

    v1.MapGroup("orders").MapOrdersEndpoints();
    v1.MapGroup("samples").MapSamplesEndpoints();
    v1.MapGroup("analyzer-samples").MapAnalyzerSamplesEndpoints();
    v1.MapGroup("worklist-items").MapWorklistItemsEndpoints();

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
