using System;
using System.Threading.Tasks;
using HC.COre.Infrastructure.EventBus;
using Serilog;

namespace HC.Core.Infrastructure.EventBus;

public class InMemoryEventBusClient(ILogger logger) : IEventsBus
{
    private readonly ILogger _logger = logger;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {

    }

    public async Task Publish<T>(T integrationEvent)
        where T : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent, "integrationEvent cannot be null");
        _logger.Information("Publishing {Event}", integrationEvent.GetType().FullName);
        await InMemoryEventBus.Instance.Publish(integrationEvent).ConfigureAwait(false);
    }

    public void Subscribe<T>(IIntegrationEventListener<T> handler)
        where T : IntegrationEvent
    {
        InMemoryEventBus.Instance.Subscribe(handler);
    }

    public void StartConsuming()
    {
    }
}
