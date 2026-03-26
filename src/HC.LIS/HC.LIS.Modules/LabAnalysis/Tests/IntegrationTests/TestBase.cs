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
using HC.LIS.Modules.LabAnalysis.Application;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Infrastructure;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase
{
    protected string? ConnectionString { get; private set; }
    protected ILogger Logger { get; private set; }
    protected ILabAnalysisModule LabAnalysisModule { get; private set; }
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
        LabAnalysisStartup.Initialize(
            ConnectionString,
            ExecutionContextAccessor,
            Logger,
            null
        );
        LabAnalysisModule = new LabAnalysisModule();
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

    private static async Task ClearDatabase(IDbConnection connection)
    {
        const string sql = @"DELETE FROM ""lab_analysis"".""InboxMessages"";
                             DELETE FROM ""lab_analysis"".""InternalCommands"";
                             DELETE FROM ""lab_analysis"".""OutboxMessages"";
                             DELETE FROM ""lab_analysis"".""mt_doc_deadletterevent"";
                             DELETE FROM ""lab_analysis"".""mt_event_progression"";
                             DELETE FROM ""lab_analysis"".""mt_events"";
                             DELETE FROM ""lab_analysis"".""mt_streams"";";

        await connection.ExecuteScalarAsync(sql).ConfigureAwait(false);
    }

    public void Dispose()
    {
        LabAnalysisStartup.Stop();
    }
}
