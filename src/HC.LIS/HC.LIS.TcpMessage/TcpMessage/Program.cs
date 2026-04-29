using System;
using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HC.Core.Application;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations;
using HC.LIS.TcpMessage;
using HC.LIS.TcpMessage.AuditLog;
using HC.LIS.TcpMessage.Configuration;
using HC.LIS.TcpMessage.Tcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

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
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(config =>
            config.AddEnvironmentVariables("ASPNETCORE_HCLIS_"))
        .UseSerilog((_, config) =>
            config
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}",
                    formatProvider: CultureInfo.InvariantCulture))
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>(cb =>
            cb.RegisterModule(new AnalyzerAutofacModule()))
        .ConfigureServices((ctx, services) =>
        {
            services.AddOptions<TcpOptions>()
                .Bind(ctx.Configuration.GetSection("Tcp"));

            // ConnectionHandler takes TcpOptions directly (not IOptions<TcpOptions>)
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<TcpOptions>>().Value);

            services.AddSingleton<IExecutionContextAccessor, SystemExecutionContextAccessor>();
            services.AddSingleton<TcpAuditLogger>();
            services.AddSingleton<ConnectionHandler>();
            services.AddHostedService<TcpListenerService>();
        })
        .Build();

    var connectionString = host.Services.GetRequiredService<IConfiguration>()["DATABASE_CONNECTION_STRING"]
        ?? throw new InvalidOperationException(
            "Missing required env var: ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING");

    var tcpOptions = host.Services.GetRequiredService<IOptions<TcpOptions>>().Value;
    var executionContext = host.Services.GetRequiredService<IExecutionContextAccessor>();

    AnalyzerStartup.Initialize(
        connectionString,
        executionContext,
        Log.Logger,
        eventBus: null,
        enableHl7Checksum: tcpOptions.EnableHl7Checksum);

    await host.RunAsync().ConfigureAwait(false);
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "HC.LIS.TcpMessage terminated unexpectedly");
}
finally
{
    AnalyzerStartup.Stop();
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
