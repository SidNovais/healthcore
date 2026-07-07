using System;
using System.Collections.Generic;

namespace HC.Core.Infrastructure.EventBus;

// Canonical mapping from integration event type to its wire representation.
// Defined once in the application entry point and shared across all module bus instances.
public sealed class EventRegistry
{
    private readonly Dictionary<Type, EventBinding> _bindings;
    private readonly Dictionary<string, Type> _typeByRoutingKey;

    private EventRegistry(Dictionary<Type, EventBinding> bindings)
    {
        _bindings = bindings;
        _typeByRoutingKey = [];
        foreach ((Type type, EventBinding binding) in bindings)
            _typeByRoutingKey[binding.RoutingKey] = type;
    }

    public bool TryGetBinding(Type eventType, out EventBinding binding)
        => _bindings.TryGetValue(eventType, out binding);

    public bool TryGetTypeByRoutingKey(string routingKey, out Type? type)
        => _typeByRoutingKey.TryGetValue(routingKey, out type);

    public sealed class RegistryBuilder
    {
        private readonly Dictionary<Type, EventBinding> _bindings = [];

        public RegistryBuilder Register<T>(string exchange, string routingKey)
            where T : IntegrationEvent
        {
            _bindings[typeof(T)] = new EventBinding(exchange, routingKey);
            return this;
        }

        public EventRegistry Build() => new(_bindings);
    }
}

public readonly record struct EventBinding(string Exchange, string RoutingKey);
