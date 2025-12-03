using System;

namespace HC.Core.Infrastructure.DomainEventsDispatching;

public interface IDomainNotificationsMapper
{
    string? GetNameByType(Type type);

    Type? GetTypeByName(string name);
}
