using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.UnitTests;

public class DomainEventsTestHelper
{
    public static IReadOnlyCollection<IDomainEvent> GetAllDomainEvents(Entity aggregate)
    {
        List<IDomainEvent> domainEvents = [];

        if (aggregate.Events != null)
        {
            domainEvents.AddRange(aggregate.Events);
        }

        FieldInfo[] fields = [
            .. aggregate.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public),
            .. (aggregate.GetType().BaseType?.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                ?? [])
        ];

        foreach (var field in fields)
        {
            var isEntity = typeof(Entity).IsAssignableFrom(field.FieldType);

            if (isEntity)
            {
                var range = field.GetValue(aggregate) is Entity entity ? GetAllDomainEvents(entity).ToList() : [];
                domainEvents.AddRange(range);
            }

            if (field.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(field.FieldType))
            {
                if (field.GetValue(aggregate) is IEnumerable enumerable)
                {
                    foreach (var en in enumerable)
                    {
                        if (en is Entity entityItem)
                        {
                            domainEvents.AddRange(GetAllDomainEvents(entityItem));
                        }
                    }
                }
            }
        }

        return domainEvents.AsReadOnly();
    }

    public static void ClearAllDomainEvents(Entity aggregate)
    {
        aggregate.ClearEvents();

        FieldInfo[] fields = [
            .. aggregate.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public),
            .. (aggregate.GetType().BaseType?.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                ?? [])
        ];

        foreach (var field in fields)
        {
            var isEntity = field.FieldType.IsAssignableFrom(typeof(Entity));

            if (isEntity)
            {
                if (field.GetValue(aggregate) is Entity entity) ClearAllDomainEvents(entity);
            }

            if (field.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(field.FieldType))
            {
                if (field.GetValue(aggregate) is IEnumerable enumerable)
                {
                    foreach (var en in enumerable)
                    {
                        if (en is Entity entityItem)
                        {
                            ClearAllDomainEvents(entityItem);
                        }
                    }
                }
            }
        }
    }
}
