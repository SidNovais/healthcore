using HC.Core.Infrastructure.EventBus;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace HC.LIS.API.Configuration.EventBus;

internal sealed class RabbitMqModuleEventBusFactory : IModuleEventBusFactory
{
    private readonly IConnection _connection;
    private readonly ModuleBusSet _buses;

    public IEventsBus TestOrders => _buses.TestOrders;
    public IEventsBus SampleCollection => _buses.SampleCollection;
    public IEventsBus Analyzer => _buses.Analyzer;
    public IEventsBus LabAnalysis => _buses.LabAnalysis;
    public IEventsBus UserAccess => _buses.UserAccess;
    public IEventsBus PatientManagement => _buses.PatientManagement;

    private RabbitMqModuleEventBusFactory(IConnection connection, ModuleBusSet buses)
    {
        _connection = connection;
        _buses = buses;
    }

    internal static async Task<RabbitMqModuleEventBusFactory> CreateAsync(
        EventBusOptions options, EventRegistry registry, Serilog.ILogger logger)
    {
        var factory = new ConnectionFactory { Uri = new Uri(options.ConnectionString) };

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
            async ct => await factory.CreateConnectionAsync(ct).ConfigureAwait(false),
            CancellationToken.None).ConfigureAwait(false);

        var buses = await ModuleBusSet
            .CreateAsync(connection, registry, logger).ConfigureAwait(false);

        return new RabbitMqModuleEventBusFactory(connection, buses);
    }

    public void StartConsuming() => _buses.StartConsuming();

    public void Dispose()
    {
        _buses.Dispose();
        _connection.Dispose();
    }
}
