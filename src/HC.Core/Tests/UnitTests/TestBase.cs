using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;

namespace HC.Core.UnitTests;

public abstract class TestBase
{
    public static T AssertPublishedDomainEvent<T>(Entity aggregate)
      where T : IDomainEvent
    {
        T domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault()
        ?? throw new InvalidOperationException($"{typeof(T).Name} event not published");
        return domainEvent;
    }

    public static void AssertDomainEventNotPublished<T>(Entity aggregate)
        where T : IDomainEvent
    {
        var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();
        domainEvent.Should().BeNull();
    }

    public static IList<T> AssertPublishedDomainEvents<T>(Entity aggregate)
        where T : IDomainEvent
    {
        var domainEvents = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().ToList();
        if (domainEvents.Count == 0)
            throw new InvalidOperationException($"{typeof(T).Name} event not published");
        return domainEvents;
    }

    public static void AssertBrokenRule<TRule>(Action testDelegate)
        where TRule : class, IBusinessRule
    {
        testDelegate.Should().Throw<BaseBusinessRuleException>().Which
          .Rule.Should().BeOfType<TRule>();
    }

    public static async void AssertBrokenRule<TRule>(Func<Task> testDelegate)
        where TRule : class, IBusinessRule
    {
        var businessRuleException = await testDelegate.Should().ThrowAsync<BaseBusinessRuleException>().ConfigureAwait(false);
        businessRuleException.Which.Rule.Should().BeOfType<TRule>();
    }

    public static T AssertPublishedDomainEvent<T>(AggregateRoot aggregate)
        where T : IDomainEvent
    {
        T domainEvent = aggregate.GetDomainEvents().OfType<T>().SingleOrDefault()
        ?? throw new InvalidOperationException($"{typeof(T).Name} event not published");
        return domainEvent;
    }
    public static IList<T> AssertPublishedDomainEvents<T>(AggregateRoot aggregate)
        where T : IDomainEvent
    {
        var domainEvents = aggregate.GetDomainEvents().OfType<T>().ToList();
        if (domainEvents.Count == 0)
            throw new InvalidOperationException($"{typeof(T).Name} event not published");
        return domainEvents;
    }

    public static void AssertDomainEventNotPublished<T>(AggregateRoot aggregate)
        where T : IDomainEvent
    {
        T? domainEvent = aggregate.GetDomainEvents().OfType<T>().SingleOrDefault();
        domainEvent.Should().BeNull();
    }
}
