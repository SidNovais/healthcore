using System.Threading.Tasks;
using HC.COre.Infrastructure.EventBus;

namespace HC.Core.Infrastructure.EventBus;

public interface IIntegrationEventListener<in TIntegrationEvent> : IIntegrationEventListener
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent integrationEvent);
}

public interface IIntegrationEventListener
{
}
