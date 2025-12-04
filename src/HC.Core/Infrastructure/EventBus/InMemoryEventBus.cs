using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HC.COre.Infrastructure.EventBus;

namespace HC.Core.Infrastructure.EventBus;

public sealed class InMemoryEventBus
{
    static InMemoryEventBus()
    {
    }

    private InMemoryEventBus()
    {
        _handlers = [];
    }

    public static InMemoryEventBus Instance { get; private set; } = new InMemoryEventBus();

    private readonly Dictionary<string, List<IIntegrationEventListener>> _handlers;

    public void Subscribe<T>(IIntegrationEventListener<T> handler)
        where T : IntegrationEvent
    {
        string? eventType = typeof(T).FullName;
        if (eventType != null)
        {
            if (_handlers.TryGetValue(eventType, out List<IIntegrationEventListener>? value))
            {
                List<IIntegrationEventListener> handlers = value;
                handlers.Add(handler);
            }
            else
                _handlers.Add(eventType, [handler]);
        }
    }

    public async Task Publish<T>(T integrationEvent)
        where T : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent, "IntegrationEvent cannot be null");
        string? eventType = integrationEvent.GetType().FullName;
        if (eventType == null)
            return;
        if (
            !_handlers.TryGetValue(eventType, out List<IIntegrationEventListener>? integrationEventHandlers)
            && integrationEventHandlers is null
        )
            return;
        foreach (IIntegrationEventListener integrationEventHandler in integrationEventHandlers)
        {
            if (integrationEventHandler is IIntegrationEventListener<T> handler)
                await handler.Handle(integrationEvent).ConfigureAwait(false);
        }
    }
}
