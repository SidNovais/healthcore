using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace HC.Core.Infrastructure.EventBus;

public sealed class RabbitMqEventBus : IEventsBus
{
    private const string DeadLetterExchangeName = "hclis.dead-letter";

    private readonly IChannel _publishChannel;
    private readonly IChannel _consumeChannel;
    private readonly string _publisherExchange;
    private readonly string _consumerQueue;
    private readonly EventRegistry _registry;
    private readonly ILogger _logger;

    // Keyed by routing key (semantic name, e.g. "sample.collected")
    private readonly Dictionary<string, List<IHandlerAdapter>> _handlers = [];

    // Tracks bindings to establish in StartConsumingAsync: (sourceExchange, routingKey)
    private readonly List<(string Exchange, string RoutingKey)> _pendingBindings = [];

    private bool _disposed;

    private RabbitMqEventBus(
        IChannel publishChannel,
        IChannel consumeChannel,
        string publisherExchange,
        string consumerQueue,
        EventRegistry registry,
        ILogger logger)
    {
        _publishChannel = publishChannel;
        _consumeChannel = consumeChannel;
        _publisherExchange = publisherExchange;
        _consumerQueue = consumerQueue;
        _registry = registry;
        _logger = logger;
    }

    // Declares the module's publisher exchange and consumer queue before returning.
    // Separate exchanges per bounded context; queue per module.
    public static async Task<RabbitMqEventBus> CreateAsync(
        IConnection connection,
        string publisherExchange,
        string consumerQueue,
        EventRegistry registry,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrEmpty(publisherExchange);
        ArgumentException.ThrowIfNullOrEmpty(consumerQueue);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(logger);

        IChannel publishChannel = await connection.CreateChannelAsync().ConfigureAwait(false);
        IChannel consumeChannel = await connection.CreateChannelAsync().ConfigureAwait(false);

        // Declare dead-letter exchange on consume channel (idempotent).
        await consumeChannel.ExchangeDeclareAsync(
            exchange: DeadLetterExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false).ConfigureAwait(false);

        // Declare this module's publisher exchange on publish channel (idempotent).
        await publishChannel.ExchangeDeclareAsync(
            exchange: publisherExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false).ConfigureAwait(false);

        // Declare this module's consumer queue with DLX routing (idempotent).
        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DeadLetterExchangeName,
        };

        await consumeChannel.QueueDeclareAsync(
            queue: consumerQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments).ConfigureAwait(false);

        return new RabbitMqEventBus(publishChannel, consumeChannel, publisherExchange, consumerQueue, registry, logger);
    }

    // Looks up the canonical exchange and routing key for T from the registry,
    // then publishes a persistent JSON message to the publisher exchange.
    public async Task Publish<T>(T integrationEvent)
        where T : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        if (!_registry.TryGetBinding(typeof(T), out EventBinding binding))
            throw new InvalidOperationException(
                $"No registry entry for {typeof(T).Name}. " +
                "Register it in the EventRegistry in Program.cs.");

        _logger.Information(
            "Publishing {EventType} to exchange {Exchange} with key {RoutingKey}",
            typeof(T).Name, binding.Exchange, binding.RoutingKey);

        ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(integrationEvent));

        var properties = new BasicProperties { Persistent = true };

        await _publishChannel.BasicPublishAsync(
            exchange: binding.Exchange,
            routingKey: binding.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body).ConfigureAwait(false);
    }

    // Records the source exchange and routing key for type T (from the registry)
    // and stores an adapter for the handler. Bindings are applied in StartConsuming().
    // Must be called before StartConsuming().
    public void Subscribe<T>(IIntegrationEventListener<T> handler)
        where T : IntegrationEvent
    {
        if (!_registry.TryGetBinding(typeof(T), out EventBinding binding))
            throw new InvalidOperationException(
                $"No registry entry for {typeof(T).Name}. " +
                "Register it in the EventRegistry in Program.cs.");

        _logger.Information(
            "Subscribing to {EventType} on exchange {Exchange} with key {RoutingKey} → queue {Queue}",
            typeof(T).Name, binding.Exchange, binding.RoutingKey, _consumerQueue);

        if (!_handlers.TryGetValue(binding.RoutingKey, out List<IHandlerAdapter>? adapters))
        {
            adapters = [];
            _handlers[binding.RoutingKey] = adapters;
            _pendingBindings.Add((binding.Exchange, binding.RoutingKey));
        }

        adapters.Add(new HandlerAdapter<T>(handler));
    }

    // Intentional fire-and-forget: all Subscribe<T> calls must precede this.
    public void StartConsuming() => _ = StartConsumingAsync();

    private async Task StartConsumingAsync()
    {
        // CA1031: a startup failure must not silently swallow exceptions;
        // we log and surface the error so the operator knows the consumer never started.
#pragma warning disable CA1031
        try
        {
            foreach ((string exchange, string routingKey) in _pendingBindings)
            {
                // Declare the source exchange idempotently so bindings work even if
                // the publishing module starts after this one.
                await _consumeChannel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false).ConfigureAwait(false);

                await _consumeChannel.QueueBindAsync(
                    queue: _consumerQueue,
                    exchange: exchange,
                    routingKey: routingKey,
                    arguments: null).ConfigureAwait(false);

                _logger.Information(
                    "Bound queue {Queue} ← {Exchange}/{RoutingKey}",
                    _consumerQueue, exchange, routingKey);
            }

            _pendingBindings.Clear();

            var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
            consumer.ReceivedAsync += OnMessageReceivedAsync;

            _ = await _consumeChannel.BasicConsumeAsync(
                queue: _consumerQueue,
                autoAck: false,
                consumerTag: string.Empty,
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer).ConfigureAwait(false);

            _logger.Information("Consumer started on queue {Queue}", _consumerQueue);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Failed to start RabbitMQ consumer on queue {Queue}; the consumer will not run",
                _consumerQueue);
        }
#pragma warning restore CA1031
    }

    private async Task OnMessageReceivedAsync(object _, BasicDeliverEventArgs ea)
    {
        string routingKey = ea.RoutingKey;

        if (!_handlers.TryGetValue(routingKey, out List<IHandlerAdapter>? adapters))
        {
            _logger.Warning(
                "No handlers registered for routing key {RoutingKey} on queue {Queue}; nacking",
                routingKey, _consumerQueue);
            await _consumeChannel.BasicNackAsync(
                ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
            return;
        }

        // Type is resolved from registry — no assembly scanning needed.
        if (!_registry.TryGetTypeByRoutingKey(routingKey, out Type? eventType) || eventType is null)
        {
            _logger.Error(
                "Registry has no type mapping for routing key {RoutingKey}; nacking",
                routingKey);
            await _consumeChannel.BasicNackAsync(
                ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
            return;
        }

        string json = Encoding.UTF8.GetString(ea.Body.Span);
        object? deserialized;

        // CA1031: a malformed payload must not crash the consumer loop.
#pragma warning disable CA1031
        try
        {
            deserialized = JsonConvert.DeserializeObject(json, eventType);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Deserialization failed for routing key {RoutingKey}; nacking",
                routingKey);
            await _consumeChannel.BasicNackAsync(
                ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
            return;
        }
#pragma warning restore CA1031

        if (deserialized is not IntegrationEvent integrationEvent)
        {
            _logger.Error(
                "Deserialized object is not an IntegrationEvent for {RoutingKey}; nacking",
                routingKey);
            await _consumeChannel.BasicNackAsync(
                ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
            return;
        }

        // CA1031: nack to dead-letter exchange on handler failure;
        // the Inbox idempotency guard prevents double-processing on redelivery.
#pragma warning disable CA1031
        try
        {
            foreach (IHandlerAdapter adapter in adapters)
                await adapter.HandleAsync(integrationEvent).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Handler threw for routing key {RoutingKey}; nacking to dead-letter exchange",
                routingKey);
            await _consumeChannel.BasicNackAsync(
                ea.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
            return;
        }
#pragma warning restore CA1031

        await _consumeChannel.BasicAckAsync(ea.DeliveryTag, multiple: false).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _publishChannel.Dispose();
        _consumeChannel.Dispose();
        GC.SuppressFinalize(this);
    }

    // ── Handler adapter bridge ───────────────────────────────────────────────
    // Allows the non-generic OnMessageReceivedAsync to call typed handlers
    // without reflection — the cast was validated at Subscribe<T>() time.

    private interface IHandlerAdapter
    {
        Task HandleAsync(IntegrationEvent integrationEvent);
    }

    private sealed class HandlerAdapter<T>(IIntegrationEventListener<T> handler) : IHandlerAdapter
        where T : IntegrationEvent
    {
        private readonly IIntegrationEventListener<T> _handler = handler;

        public async Task HandleAsync(IntegrationEvent integrationEvent)
        {
            if (integrationEvent is T typedEvent)
                await _handler.Handle(typedEvent).ConfigureAwait(false);
        }
    }
}
