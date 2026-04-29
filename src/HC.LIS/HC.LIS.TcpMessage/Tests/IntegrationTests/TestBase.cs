using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using HC.Core.IntegrationTests;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations;
using HC.LIS.TcpMessage.AuditLog;
using HC.LIS.TcpMessage.Configuration;
using HC.LIS.TcpMessage.IntegrationTests.Helpers;
using HC.LIS.TcpMessage.Tcp;

namespace HC.LIS.TcpMessage.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase : IDisposable
{
    private bool _disposed;
    private readonly TcpListenerService _service;

    protected string ConnectionString { get; }
    protected IAnalyzerModule AnalyzerModule { get; }
    protected int BoundPort { get; }

    public TestBase(bool enableMllpChecksum = false, bool enableHl7Checksum = false)
    {
        const string env = "ASPNETCORE_HCLIS_IntegrationTests_ConnectionString";
        ConnectionString = EnvironmentVariablesProvider.GetVariable(env)
            ?? throw new InvalidOperationException($"Set environment variable: {env}");

        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            ClearDatabase(conn).GetAwaiter().GetResult();
        }

        var logger = new LoggerConfiguration().Enrich.FromLogContext().CreateLogger();
        var executionContext = new ExecutionContextMock(Guid.CreateVersion7(), "tcpmessage-system");

        AnalyzerStartup.Initialize(
            ConnectionString,
            executionContext,
            logger,
            eventBus: null,
            enableHl7Checksum: enableHl7Checksum);

        var analyzerModule = new AnalyzerModule();
        AnalyzerModule = analyzerModule;

        var tcpOptions = Options.Create(new TcpOptions
        {
            Port = 0,
            EnableMllpChecksum = enableMllpChecksum,
            EnableHl7Checksum = enableHl7Checksum
        });

        var auditLogger = new TcpAuditLogger(NullLogger<TcpAuditLogger>.Instance);
        var handler = new ConnectionHandler(
            analyzerModule,
            auditLogger,
            tcpOptions.Value,
            NullLogger<ConnectionHandler>.Instance);

        _service = new TcpListenerService(tcpOptions, handler, NullLogger<TcpListenerService>.Instance);
        _service.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
        _service.ListenerStarted.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        BoundPort = _service.BoundPort;
    }

    protected static async Task<T?> GetEventually<T>(IProbe<T> probe, int timeoutMs)
        where T : class
    {
        var poller = new Poller(timeoutMs);
        return await poller.GetAsync(probe).ConfigureAwait(false);
    }

    private static async Task ClearDatabase(IDbConnection connection)
    {
        const string sql = @"
            DELETE FROM ""analyzer"".""InboxMessages"";
            DELETE FROM ""analyzer"".""InternalCommands"";
            DELETE FROM ""analyzer"".""OutboxMessages"";
            DELETE FROM ""analyzer"".""analyzer_sample_exam_details"";
            DELETE FROM ""analyzer"".""analyzer_sample_details"";
            DELETE FROM ""analyzer"".""mt_doc_deadletterevent"";
            DELETE FROM ""analyzer"".""mt_event_progression"";
            DELETE FROM ""analyzer"".""mt_events"";
            DELETE FROM ""analyzer"".""mt_streams"";";

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
            _service.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            _service.Dispose();
            AnalyzerStartup.Stop();
        }
        _disposed = true;
    }

    ~TestBase() => Dispose(false);
}
