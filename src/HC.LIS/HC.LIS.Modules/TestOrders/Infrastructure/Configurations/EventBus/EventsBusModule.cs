using Autofac;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.EventBus;

internal class EventsBusModule(IEventsBus? eventsBus) : Autofac.Module
{
    private readonly IEventsBus? _eventsBus = eventsBus;

    protected override void Load(ContainerBuilder builder)
    {
        if (_eventsBus != null)
        {
            builder.RegisterInstance(_eventsBus).SingleInstance();
        }
        else
        {
            builder.RegisterType<InMemoryEventBusClient>()
                .As<IEventsBus>()
                .SingleInstance();
        }
    }
}
