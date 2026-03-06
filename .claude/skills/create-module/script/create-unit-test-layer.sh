#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-unit-test-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}/Tests

mkdir -p ${BASE_MODULE_DIR}/UnitTests

cat > "${BASE_MODULE_DIR}/UnitTests/${ROOT_NS}.Modules.${MODULE_NAME}.UnitTests.csproj" << 'CSPROJEOF'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
CSPROJEOF

cat > "${BASE_MODULE_DIR}/UnitTests/DomainEventsTestHelper.cs" << EOF
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.Core.Domain;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.UnitTests;

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
EOF

cat > "${BASE_MODULE_DIR}/UnitTests/TestBase.cs" << EOF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.UnitTests;

public abstract class TestBase
{
    public static T AssertPublishedDomainEvent<T>(Entity aggregate)
        where T : IDomainEvent
    {
        var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();

        if (domainEvent == null)
        {
            throw new InvalidOperationException(\$"{typeof(T).Name} event not published");
        }

        return domainEvent;
    }

    public static void AssertDomainEventNotPublished<T>(Entity aggregate)
        where T : IDomainEvent
    {
        var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();
        domainEvent.Should().BeNull();
    }

    public static IReadOnlyCollection<T> AssertPublishedDomainEvents<T>(Entity aggregate)
        where T : IDomainEvent
    {
        var domainEvents = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().ToList();

        if (domainEvents.Count == 0)
        {
            throw new InvalidOperationException(\$"{typeof(T).Name} event not published");
        }

        return domainEvents.AsReadOnly();
    }

    public static void AssertBrokenRule<TRule>(Action testDelegate)
        where TRule : class, IBusinessRule
    {
        testDelegate.Should().Throw<BaseBusinessRuleException>().Which
            .Rule.Should().BeOfType<TRule>();
    }

    public static async Task AssertBrokenRuleAsync<TRule>(Func<Task> testDelegate)
        where TRule : class, IBusinessRule
    {
        var businessRuleException = await testDelegate.Should().ThrowAsync<BaseBusinessRuleException>().ConfigureAwait(false);
        businessRuleException.Which.Rule.Should().BeOfType<TRule>();
    }
}
EOF
