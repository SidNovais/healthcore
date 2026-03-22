using System.Globalization;
using FluentMigrator.Runner;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TestOrdersMartenConfig = HC.LIS.Modules.TestOrders.Infrastructure.Configurations.DataAccess.MartenConfig;
using SampleCollectionMartenConfig = HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.DataAccess.MartenConfig;

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

string connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING")!;

IDocumentStore testOrdersStore = TestOrdersMartenConfig.BuildDocumentStore(connectionString);
testOrdersStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync().Wait();

IDocumentStore sampleCollectionStore = SampleCollectionMartenConfig.BuildDocumentStore(connectionString);
sampleCollectionStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync().Wait();

Log.Logger.Information("Migration executed!");
Log.CloseAndFlush();
ServiceProvider CreateServices()
{
    string? dbConnectionString = Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING");
    return new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddPostgres()
            .WithGlobalConnectionString(dbConnectionString)
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
