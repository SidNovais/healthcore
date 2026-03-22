using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using MediatR;
using Npgsql;
using Serilog;
using HC.Core.Application;
using HC.Core.Domain;
using HC.Core.IntegrationTests;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.Infrastructure;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase : IDisposable
{
    #pragma warning disable CA1805
    private bool _disposed = false;
    #pragma warning restore CA1805
    protected string? ConnectionString { get; private set; }
    protected ILogger Logger { get; private set; }
    protected ISampleCollectionModule SampleCollectionModule { get; private set; }
    protected IExecutionContextAccessor ExecutionContextAccessor { get; private set; }

    public TestBase(Guid UserId, string RoleScopeType = "Customer")
    {
        const string connectionStringEnvironmentVariable = "ASPNETCORE_HCLIS_IntegrationTests_ConnectionString";
        ConnectionString = EnvironmentVariablesProvider.GetVariable(connectionStringEnvironmentVariable);
        if (ConnectionString == null)
        {
            throw new InvalidOperationException(
                $"Define connection string to integration tests database using environment variable: {connectionStringEnvironmentVariable}");
        }

        using (var sqlConnection = new NpgsqlConnection(ConnectionString))
        {
            ClearDatabase(sqlConnection).GetAwaiter().GetResult();
        }

        Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .CreateLogger();

        ExecutionContextAccessor = new ExecutionContextMock(UserId, "user");
        SampleCollectionStartup.Initialize(
            ConnectionString,
            ExecutionContextAccessor,
            Logger,
            null
        );
        SampleCollectionModule = new SampleCollectionModule();
    }

    protected static void AssertBrokenRule<TRule>(Action testDelegate)
        where TRule : class, IBusinessRule
    {
        testDelegate.Should().Throw<BaseBusinessRuleException>().Which
            .Rule.Should().BeOfType<TRule>();
    }

    protected async Task<T?> GetLastOutboxMessage<T>()
        where T : class, INotification
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            var messages = await OutboxMessagesHelper.GetOutboxMessages(connection).ConfigureAwait(false);
            return OutboxMessagesHelper.Deserialize<T>(messages.Last(x => x.Type == typeof(T).Name));
        }
    }

    public static async Task<T?> GetEventually<T>(IProbe<T> probe, int timeout)
        where T : class
    {
        Poller poller = new(timeout);
        return await poller.GetAsync(probe).ConfigureAwait(false);
    }

    private static async Task ClearDatabase(IDbConnection connection)
    {
        const string sql = @"
            DELETE FROM ""sample_collection"".""InboxMessages"";
            DELETE FROM ""sample_collection"".""InternalCommands"";
            DELETE FROM ""sample_collection"".""OutboxMessages"";
            DELETE FROM ""sample_collection"".""CollectionRequestDetails"";
            DELETE FROM ""sample_collection"".""SampleDetails"";
            DELETE FROM ""sample_collection"".""mt_doc_deadletterevent"";
            DELETE FROM ""sample_collection"".""mt_event_progression"";
            DELETE FROM ""sample_collection"".""mt_events"";
            DELETE FROM ""sample_collection"".""mt_streams"";";

        await connection.ExecuteScalarAsync(sql).ConfigureAwait(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            SampleCollectionStartup.Stop();
        }
        _disposed = true;
    }

    ~TestBase()
    {
        Dispose(false);
    }
}
