using System.Globalization;
using FluentMigrator.Runner;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.DataAccess;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        formatProvider: CultureInfo.CurrentCulture,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger()
;
IServiceProvider serviceProvider = CreateServices();
using (IServiceScope scope = serviceProvider.CreateScope())
{
    UpdateDatabase(scope.ServiceProvider);
}
IDocumentStore store = MartenConfig.BuildDocumentStore(Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING")!);
store.Storage.ApplyAllConfiguredChangesToDatabaseAsync().Wait();
Log.Logger.Information("Migration executed!");
Log.CloseAndFlush();
ServiceProvider CreateServices()
{
    string? connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING");
    return new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddPostgres()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(Program).Assembly).For.Migrations())
        .AddLogging(lb =>
        {
            lb.AddSerilog();
        })
        .BuildServiceProvider(false);
}
void UpdateDatabase(IServiceProvider serviceProvider)
{
    IMigrationRunner runner = serviceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}
