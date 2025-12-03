using System;

namespace HC.Core.Infrastructure.DomainEventsDispatching;

public class DomainNotificationsMapper(
    BiMap domainNotificationsMap
) : IDomainNotificationsMapper
{
    private readonly BiMap _domainNotificationsMap = domainNotificationsMap;

    public string? GetNameByType(Type type)
    {
        return _domainNotificationsMap.TryGetBySecond(type, out var name) ? name : null;
    }

    public Type? GetTypeByName(string name)
    {
        return _domainNotificationsMap.TryGetByFirst(name, out var type) ? type : null;
    }
}
