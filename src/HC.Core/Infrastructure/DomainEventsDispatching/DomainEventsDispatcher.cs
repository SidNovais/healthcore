using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using HC.Core.Application;
using HC.Core.Application.Events;
using HC.Core.Domain;
using HC.Core.Infrastructure.Outbox;
using HC.Core.Infrastructure.Serialization;
using MediatR;
using Newtonsoft.Json;

namespace HC.Core.Infrastructure.DomainEventsDispatching;

public class DomainEventsDispatcher(
    IMediator mediator,
    ILifetimeScope scope,
    IOutbox outbox,
    IDomainEventsAccessor domainEventsProvider,
    IDomainNotificationsMapper domainNotificationsMapper
) : IDomainEventsDispatcher
{
    private readonly IMediator _mediator = mediator;

    private readonly ILifetimeScope _scope = scope;

    private readonly IOutbox _outbox = outbox;

    private readonly IDomainEventsAccessor _domainEventsProvider = domainEventsProvider;

    private readonly IDomainNotificationsMapper _domainNotificationsMapper = domainNotificationsMapper;

    public async Task DispatchEventsAsync()
    {
        IReadOnlyCollection<IDomainEvent> domainEvents = _domainEventsProvider.GetAllDomainEvents();

        var domainEventNotifications = new List<IDomainEventNotification<IDomainEvent>>();
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type domainEvenNotificationType = typeof(IDomainEventNotification<>);
            Type domainNotificationWithGenericType = domainEvenNotificationType.MakeGenericType(domainEvent.GetType());
            object? domainNotification = _scope.ResolveOptional(domainNotificationWithGenericType, new List<Parameter>
                {
                    new NamedParameter("domainEvent", domainEvent),
                    new NamedParameter("id", domainEvent.Id)
                });

            if (domainNotification is IDomainEventNotification<IDomainEvent> convertedNotification)
                domainEventNotifications.Add(convertedNotification);
        }

        _domainEventsProvider.ClearAllDomainEvents();

        foreach (IDomainEvent domainEvent in domainEvents)
            await _mediator.Publish(domainEvent).ConfigureAwait(false);

        foreach (IDomainEventNotification<IDomainEvent> domainEventNotification in domainEventNotifications)
        {
            string? type = _domainNotificationsMapper.GetNameByType(domainEventNotification.GetType());
            string data = JsonConvert.SerializeObject(domainEventNotification, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            });

            if (type != null)
            {
                var outboxMessage = new OutboxMessage(
                    domainEventNotification.Id,
                    domainEventNotification.EventNotification.OcurredAt,
                    type,
                    data
                );
                _outbox.Add(outboxMessage);
            }
        }
    }
}
