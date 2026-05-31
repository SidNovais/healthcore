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
using HC.LIS.Modules.PatientManagement.Application;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Infrastructure;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations;

namespace HC.LIS.Modules.PatientManagement.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase : IDisposable
{
    #pragma warning disable CA1805
    private bool _disposed = false;
    #pragma warning restore CA1805

    protected string? ConnectionString { get; private set; }
    protected ILogger Logger { get; private set; }
    protected IPatientManagementModule PatientManagementModule { get; private set; }
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
        PatientManagementStartup.Initialize(
            ConnectionString,
            ExecutionContextAccessor,
            Logger,
            null
        );
        PatientManagementModule = new PatientManagementModule();
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
        DELETE FROM ""patient_management"".""InboxMessages"";
        DELETE FROM ""patient_management"".""InternalCommands"";
        DELETE FROM ""patient_management"".""OutboxMessages"";
        DELETE FROM ""patient_management"".""PatientDetails"";
        DO $$ BEGIN
          IF EXISTS (SELECT 1 FROM information_schema.tables
                     WHERE table_schema = 'patient_management' AND table_name = 'mt_events') THEN
            DELETE FROM ""patient_management"".""mt_doc_deadletterevent"";
            DELETE FROM ""patient_management"".""mt_event_progression"";
            DELETE FROM ""patient_management"".""mt_events"";
            DELETE FROM ""patient_management"".""mt_streams"";
          END IF;
        END $$;
        ";

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
            PatientManagementStartup.Stop();
            SystemClock.Clear();
        }
        _disposed = true;
    }

    ~TestBase()
    {
        Dispose(false);
    }
}
