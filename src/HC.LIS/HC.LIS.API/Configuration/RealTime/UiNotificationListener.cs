using HC.Core.Infrastructure.EventBus;
using HC.Core.Infrastructure.RealTime;

namespace HC.LIS.API.Configuration.RealTime;

/// <summary>
/// Bridges a single integration-event type to the <see cref="IUiNotificationHub"/>: it maps the
/// event to a browser-ready <see cref="UiNotification"/> and publishes it. A <c>null</c> map result
/// means "nothing to relay for this event".
/// </summary>
internal sealed class UiNotificationListener<TEvent>(
    IUiNotificationHub hub,
    Func<TEvent, UiNotification?> map) : IIntegrationEventListener<TEvent>
    where TEvent : IntegrationEvent
{
    public Task Handle(TEvent integrationEvent)
    {
        UiNotification? notification = map(integrationEvent);
        if (notification is not null)
            hub.Publish(notification);

        return Task.CompletedTask;
    }
}
