using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations;
using Npgsql;
using Serilog;

namespace HC.LIS.Tests.IntegrationEvents;

[Collection("IntegrationTests")]
public abstract class TestBase : IAsyncLifetime
{
    protected string ConnectionString { get; private set; } = null!;
    protected ExecutionContextMock ExecutionContext { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        const string envVar = "ASPNETCORE_HCLIS_IntegrationTests_ConnectionString";
        ConnectionString = EnvironmentVariablesProvider.GetVariable(envVar)
            ?? throw new InvalidOperationException($"Set environment variable: {envVar}");

        ExecutionContext = new ExecutionContextMock(Guid.NewGuid());

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await DatabaseCleaner.ClearAllAsync(connection);

        var logger = new LoggerConfiguration().Enrich.FromLogContext().CreateLogger();

        TestOrdersStartup.Initialize(ConnectionString, ExecutionContext, logger, eventBus: null);
        SampleCollectionStartup.Initialize(ConnectionString, ExecutionContext, logger, eventBus: null);
        AnalyzerStartup.Initialize(ConnectionString, ExecutionContext, logger, eventBus: null);
        LabAnalysisStartup.Initialize(ConnectionString, ExecutionContext, logger, eventBus: null);
    }

    public virtual Task DisposeAsync()
    {
        TestOrdersStartup.Stop();
        SampleCollectionStartup.Stop();
        AnalyzerStartup.Stop();
        LabAnalysisStartup.Stop();
        return Task.CompletedTask;
    }
}
