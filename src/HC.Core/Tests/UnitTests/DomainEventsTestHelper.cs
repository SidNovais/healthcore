using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.Core.Domain;

namespace HC.Core.UnitTests;

public static class DomainEventsTestHelper
{
    public static IList<IDomainEvent> GetAllDomainEvents(Entity aggregate)
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
                List<IDomainEvent> range = field.GetValue(aggregate) is Entity entity ? GetAllDomainEvents(entity).ToList() : [];
                domainEvents.AddRange(range);
            }

            if (field.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(field.FieldType))
            {
                if (field.GetValue(aggregate) is IEnumerable enumerable)
                {
                    foreach (object? en in enumerable)
                    {
                        if (en is Entity entityItem)
                        {
                            domainEvents.AddRange(GetAllDomainEvents(entityItem));
                        }
                    }
                }
            }
        }

        return domainEvents;
    }

    public static void ClearAllDomainEvents(Entity aggregate)
    {
        aggregate.ClearEvents();
        FieldInfo[] fields = [
            .. aggregate.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public),
                .. (aggregate.GetType().BaseType?.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    ?? [])
        ];

        foreach (FieldInfo field in fields)
        {
            bool isEntity = field.FieldType.IsAssignableFrom(typeof(Entity));

            if (isEntity)
            {
                if (field.GetValue(aggregate) is Entity entity) ClearAllDomainEvents(entity);
            }

            if (field.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(field.FieldType))
            {
                if (field.GetValue(aggregate) is IEnumerable enumerable)
                {
                    foreach (object? en in enumerable)
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
