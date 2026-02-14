using System;
using System.Collections.Generic;
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
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Infrastructure;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;

namespace HC.LIS.Modules.TestOrders.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase : IDisposable
{
    protected string? ConnectionString { get; private set; }
    protected ILogger Logger { get; private set; }
    protected ITestOrdersModule TestOrdersModule { get; private set; }
    protected IExecutionContextAccessor ExecutionContextAccessor { get; private set; }
    #pragma warning disable CA1805
    private bool _disposed = false;

    #pragma warning restore CA1805

    public TestBase(Guid UserId)
    {
        const string connectionStringEnvironmentVariable = "ASPNETCORE_HCLIS_IntegrationTests_ConnectionString";
        ConnectionString = EnvironmentVariablesProvider.GetVariable(connectionStringEnvironmentVariable);
        if (ConnectionString == null)
        {
            throw new InvalidProgramException(
                $"Define connection string to integration tests database using environment variable: {connectionStringEnvironmentVariable}");
        }
        using (var sqlConnection = new NpgsqlConnection(ConnectionString))
        {
            ClearDatabase(sqlConnection).GetAwaiter().GetResult();
        }

        Logger = new LoggerConfiguration()
          .Enrich.FromLogContext()
          .CreateLogger()
        ;
        ExecutionContextAccessor = new ExecutionContextMock(UserId);
        TestOrdersStartup.Initialize(
          ConnectionString,
          ExecutionContextAccessor,
          Logger,
          null
        );
        TestOrdersModule = new TestOrdersModule();
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
            List<OutboxMessageDto> messages = await OutboxMessagesHelper.GetOutboxMessages(connection).ConfigureAwait(false);

            return OutboxMessagesHelper.Deserialize<T>(messages.Last(message => message.Type == typeof(T).Name));
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
        DELETE FROM ""test_orders"".""InboxMessages"";
        DELETE FROM ""test_orders"".""InternalCommands"";
        DELETE FROM ""test_orders"".""OutboxMessages"";
        DELETE FROM ""test_orders"".""OrderDetails"";
        DELETE FROM ""test_orders"".""OrderItemDetails"";
        DELETE FROM ""test_orders"".""mt_doc_deadletterevent"";
        DELETE FROM ""test_orders"".""mt_event_progression"";
        DELETE FROM ""test_orders"".""mt_events"";
        DELETE FROM ""test_orders"".""mt_streams"";
        "
        ;

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
            TestOrdersStartup.Stop();
            SystemClock.Clear();
        }
        _disposed = true;
    }

    ~TestBase()
    {
        Dispose(false);
    }
}
