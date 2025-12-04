using System;
using System.Threading.Tasks;
using HC.COre.Infrastructure.EventBus;

namespace HC.Core.Infrastructure.EventBus;

public interface IEventsBus : IDisposable
{
    Task Publish<T>(T integrationEvent)
        where T : IntegrationEvent;

    void Subscribe<T>(IIntegrationEventListener<T> handler)
        where T : IntegrationEvent;

    void StartConsuming();
}
