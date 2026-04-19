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
using HC.LIS.Modules.UserAccess.Application;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Infrastructure;
using HC.LIS.Modules.UserAccess.Infrastructure.Configurations;

namespace HC.LIS.Modules.UserAccess.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase : IDisposable
{
#pragma warning disable CA1805
    private bool _disposed = false;
#pragma warning restore CA1805

    protected string? ConnectionString { get; private set; }
    protected ILogger Logger { get; private set; }
    protected IUserAccessModule UserAccessModule { get; private set; }
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
        UserAccessStartup.Initialize(
            ConnectionString,
            ExecutionContextAccessor,
            Logger,
            null
        );
        UserAccessModule = new UserAccessModule();
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
        const string sql = @"DELETE FROM user_access.audit_log;
                             DELETE FROM user_access.users;
                             DELETE FROM ""user_access"".""InboxMessages"";
                             DELETE FROM ""user_access"".""InternalCommands"";
                             DELETE FROM ""user_access"".""OutboxMessages"";";

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
            UserAccessStartup.Stop();
        }
        _disposed = true;
    }

    ~TestBase()
    {
        Dispose(false);
    }
}
