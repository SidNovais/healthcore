using HC.Core.Infrastructure.EventBus;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace HC.LIS.API.Configuration.EventBus;

internal sealed class RabbitMqModuleBusProvider : IModuleBusProvider
{
    private readonly IConnection _connection;
    private readonly RabbitMqEventBus _testOrders;
    private readonly RabbitMqEventBus _sampleCollection;
    private readonly RabbitMqEventBus _analyzer;
    private readonly RabbitMqEventBus _labAnalysis;
    private readonly RabbitMqEventBus _userAccess;
    private readonly RabbitMqEventBus _patientManagement;
    private readonly RabbitMqEventBus _uiNotifications;
    private bool _disposed;

    // The UI feed is disposable state — cap it and drop the oldest / expire quickly so it can
    // never grow unbounded while the API is offline; a reconnecting browser re-fetches anyway.
    private static readonly Dictionary<string, object?> s_uiQueueArguments = new()
    {
        ["x-max-length"] = 1000,
        ["x-overflow"] = "drop-head",
        ["x-message-ttl"] = 60_000,
    };

    public IEventsBus TestOrders => _testOrders;
    public IEventsBus SampleCollection => _sampleCollection;
    public IEventsBus Analyzer => _analyzer;
    public IEventsBus LabAnalysis => _labAnalysis;
    public IEventsBus UserAccess => _userAccess;
    public IEventsBus PatientManagement => _patientManagement;
    public IEventsBus UiNotifications => _uiNotifications;

    private RabbitMqModuleBusProvider(
        IConnection connection,
        RabbitMqEventBus testOrders,
        RabbitMqEventBus sampleCollection,
        RabbitMqEventBus analyzer,
        RabbitMqEventBus labAnalysis,
        RabbitMqEventBus userAccess,
        RabbitMqEventBus patientManagement,
        RabbitMqEventBus uiNotifications)
    {
        _connection = connection;
        _testOrders = testOrders;
        _sampleCollection = sampleCollection;
        _analyzer = analyzer;
        _labAnalysis = labAnalysis;
        _userAccess = userAccess;
        _patientManagement = patientManagement;
        _uiNotifications = uiNotifications;
    }

    internal static async Task<RabbitMqModuleBusProvider> CreateAsync(
        EventBusOptions options, EventRegistry registry, Serilog.ILogger logger)
    {
        var connectionFactory = new ConnectionFactory { Uri = new Uri(options.ConnectionString) };

        var retryPipeline = new ResiliencePipelineBuilder<IConnection>()
            .AddRetry(new RetryStrategyOptions<IConnection>
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger.Warning(
                        "RabbitMQ connection attempt {Attempt} failed: {Message}. Retrying...",
                        args.AttemptNumber + 1,
                        args.Outcome.Exception?.Message);
                    return default;
                },
            })
            .Build();

        var connection = await retryPipeline.ExecuteAsync(
            async ct => await connectionFactory.CreateConnectionAsync(ct).ConfigureAwait(false),
            CancellationToken.None).ConfigureAwait(false);

        var testOrders = await RabbitMqEventBus.CreateAsync(
            connection, "orders.events", "hclis.test_orders", registry, logger).ConfigureAwait(false);
        var sampleCollection = await RabbitMqEventBus.CreateAsync(
            connection, "sample_collection.events", "hclis.sample_collection", registry, logger).ConfigureAwait(false);
        var analyzer = await RabbitMqEventBus.CreateAsync(
            connection, "analyzer.events", "hclis.analyzer", registry, logger).ConfigureAwait(false);
        var labAnalysis = await RabbitMqEventBus.CreateAsync(
            connection, "lab_analysis.events", "hclis.lab_analysis", registry, logger).ConfigureAwait(false);
        var userAccess = await RabbitMqEventBus.CreateAsync(
            connection, "user_access.events", "hclis.user_access", registry, logger).ConfigureAwait(false);
        var patientManagement = await RabbitMqEventBus.CreateAsync(
            connection, "patient_management.events", "hclis.patient_management", registry, logger).ConfigureAwait(false);

        // Consume-only bus for the SSE feed; its publisher exchange is declared but never used.
        var uiNotifications = await RabbitMqEventBus.CreateAsync(
            connection, "hclis.ui_notifications", "hclis.ui_notifications", registry, logger,
            s_uiQueueArguments).ConfigureAwait(false);

        return new RabbitMqModuleBusProvider(
            connection, testOrders, sampleCollection, analyzer, labAnalysis, userAccess, patientManagement,
            uiNotifications);
    }

    public void StartConsuming()
    {
        _testOrders.StartConsuming();
        _sampleCollection.StartConsuming();
        _analyzer.StartConsuming();
        _labAnalysis.StartConsuming();
        _userAccess.StartConsuming();
        _patientManagement.StartConsuming();
        _uiNotifications.StartConsuming();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _testOrders.Dispose();
        _sampleCollection.Dispose();
        _analyzer.Dispose();
        _labAnalysis.Dispose();
        _userAccess.Dispose();
        _patientManagement.Dispose();
        _uiNotifications.Dispose();
        _connection.Dispose();
    }
}
