using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.DomainEventsDispatching;
using HC.Core.Infrastructure.Outbox;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.AggregateStore;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;

internal class OutboxModule(BiMap domainNotificationsMap) : Autofac.Module
{
    private readonly BiMap _domainNotificationsMap = domainNotificationsMap;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SqlOutboxAccessor>()
            .As<IOutbox>()
            .FindConstructorsWith(new AllConstructorFinder())
            .InstancePerLifetimeScope();

        CheckMappings();

        builder.RegisterType<DomainNotificationsMapper>()
            .As<IDomainNotificationsMapper>()
            .FindConstructorsWith(new AllConstructorFinder())
            .WithParameter("domainNotificationsMap", _domainNotificationsMap)
            .SingleInstance();
    }

    private void CheckMappings()
    {
        var domainEventNotifications = Assemblies.Application
            .GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IDomainEventNotification)))
            .ToList();

        List<Type> notMappedNotifications = [];
        foreach (Type? domainEventNotification in domainEventNotifications)
        {
            _domainNotificationsMap.TryGetBySecond(domainEventNotification, out var name);

            if (name == null)
            {
                notMappedNotifications.Add(domainEventNotification);
            }
        }

        if (notMappedNotifications.Count > 0)
            throw new NotMappedNotificationsException($"Domain Event Notifications {notMappedNotifications.Select(x => x.FullName).Aggregate(static (x, y) => x + "," + y)} not mapped");
    }
}

public class NotMappedNotificationsException : Exception
{
    public NotMappedNotificationsException()
    {
    }
    public NotMappedNotificationsException(string message) : base(message)
    {
    }

    public NotMappedNotificationsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
