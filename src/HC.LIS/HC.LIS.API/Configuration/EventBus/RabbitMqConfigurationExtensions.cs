using System.Globalization;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace HC.LIS.API.Configuration.EventBus;

internal static class RabbitMqConfigurationExtensions
{
    internal static async Task<IConnection> CreateHcLisRabbitMqConnectionAsync(
        this IConfiguration config, Serilog.ILogger logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RABBITMQ_HOST"] ?? "localhost",
            Port = int.Parse(config["RABBITMQ_PORT"] ?? "5672", CultureInfo.InvariantCulture),
            UserName = config["RABBITMQ_USERNAME"] ?? "guest",
            Password = config["RABBITMQ_PASSWORD"] ?? "guest",
        };

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

        return await retryPipeline.ExecuteAsync(
            async ct => await factory.CreateConnectionAsync(ct).ConfigureAwait(false),
            CancellationToken.None).ConfigureAwait(false);
    }
}
