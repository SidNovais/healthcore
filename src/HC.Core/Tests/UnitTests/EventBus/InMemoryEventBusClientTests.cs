using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Infrastructure.EventBus;
using NSubstitute;
using Serilog;

namespace HC.Core.UnitTests.EventBus;

public sealed class InMemoryEventBusClientTests
{
    // Guards the in-memory-bus mode of the SSE feed: a UI listener subscribed on its own
    // InMemoryEventBusClient must still receive events published by any other module's client,
    // because they all share the process-wide InMemoryEventBus.Instance keyed by event type.
    [Fact]
    public async Task SubscriberOnOneClientReceivesEventPublishedByAnotherClient()
    {
        ILogger logger = Substitute.For<ILogger>();
        using var publisher = new InMemoryEventBusClient(logger);
        using var consumer = new InMemoryEventBusClient(logger);
        var listener = new CapturingListener();
        consumer.Subscribe(listener);

        var published = new ProbeIntegrationEvent(Guid.NewGuid());
        await publisher.Publish(published).ConfigureAwait(true);

        listener.Received.Should().ContainSingle().Which.Id.Should().Be(published.Id);
    }

    private sealed class ProbeIntegrationEvent(Guid id) : IntegrationEvent(id, default);

    private sealed class CapturingListener : IIntegrationEventListener<ProbeIntegrationEvent>
    {
        public List<ProbeIntegrationEvent> Received { get; } = [];

        public Task Handle(ProbeIntegrationEvent integrationEvent)
        {
            Received.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
