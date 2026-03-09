using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.ArchTests.Domain;

public class DomainTests : TestBase
{
    [Fact]
    public void DomainEventShouldBeImmutable()
    {
        var types = Types.InAssembly(DomainAssembly)
            .That()
                .Inherit(typeof(DomainEvent))
                    .Or()
                .ImplementInterface(typeof(IDomainEvent))
            .GetTypes();
        AssertAreImmutable(types);
    }

    [Fact]
    public void ValueObjectShouldBeImmutable()
    {
        var types = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(ValueObject))
            .GetTypes();
        AssertAreImmutable(types);
    }

    [Fact]
    public void BusinessRuleShouldHaveRulePostfix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IBusinessRule))
            .Should().HaveNameEndingWith("Rule", StringComparison.OrdinalIgnoreCase)
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void DomainEventShouldHaveDomainEventPostfix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Or()
            .Inherit(typeof(DomainEvent))
            .Should().HaveNameEndingWith("DomainEvent", StringComparison.OrdinalIgnoreCase)
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void EntityShouldNotHavePublicMembersWhenIsNotAggregateRoot()
    {
        var types = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .And().DoNotImplementInterface(typeof(IAggregateRoot)).GetTypes();

        const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                          BindingFlags.Public |
                                          BindingFlags.Instance |
                                          BindingFlags.Static;

        var failingTypes = new List<Type>();
        foreach (var type in types)
        {
            var publicFields = type.GetFields(bindingFlags);
            var publicProperties = type.GetProperties(bindingFlags);
            var publicMethods = type.GetMethods(bindingFlags);

            if (publicFields.Length != 0 || publicProperties.Length != 0 || publicMethods.Length != 0)
            {
                failingTypes.Add(type);
            }
        }
        AssertFailingTypes(failingTypes);
    }

    [Fact]
    public void EntityShouldNotHaveReferenceToOtherAggregateRoot()
    {
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity)).GetTypes();
        var aggregateRoots = Types.InAssembly(DomainAssembly)
            .That().ImplementInterface(typeof(IAggregateRoot)).GetTypes().ToList();

        const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                          BindingFlags.NonPublic |
                                          BindingFlags.Instance;

        var failingTypes = new List<Type>();
        foreach (var type in entityTypes)
        {
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (aggregateRoots.Contains(field.FieldType) ||
                    field.FieldType.GenericTypeArguments.Any(x => aggregateRoots.Contains(x)))
                {
                    failingTypes.Add(type);
                    break;
                }
            }

            var properties = type.GetProperties(bindingFlags);
            foreach (var property in properties)
            {
                if (aggregateRoots.Contains(property.PropertyType) ||
                    property.PropertyType.GenericTypeArguments.Any(x => aggregateRoots.Contains(x)))
                {
                    failingTypes.Add(type);
                    break;
                }
            }
        }
        AssertFailingTypes(failingTypes);
    }

    [Fact]
    public void EntityShouldHaveParameterlessPrivateConstructor()
    {
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity)).GetTypes();
        var failingTypes = new List<Type>();
        foreach (var entityType in entityTypes)
        {
            bool hasPrivateParameterlessConstructor = false;
            var constructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var constructorInfo in constructors)
            {
                if (constructorInfo.IsPrivate && constructorInfo.GetParameters().Length == 0)
                {
                    hasPrivateParameterlessConstructor = true;
                }
            }

            if (!hasPrivateParameterlessConstructor)
            {
                failingTypes.Add(entityType);
            }
        }
        AssertFailingTypes(failingTypes);
    }

    [Fact]
    public void DomainObjectShouldHaveOnlyPrivateConstructors()
    {
        var domainObjectTypes = Types.InAssembly(DomainAssembly)
            .That()
                .Inherit(typeof(Entity))
                .Or()
                .Inherit(typeof(ValueObject))
            .GetTypes();
        var failingTypes = new List<Type>();
        foreach (var domainObjectType in domainObjectTypes)
        {
            var constructors = domainObjectType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var constructorInfo in constructors)
            {
                if (!constructorInfo.IsPrivate)
                {
                    failingTypes.Add(domainObjectType);
                }
            }
        }
        AssertFailingTypes(failingTypes);
    }

    [Fact]
    public void ValueObjectShouldHavePrivateConstructorWithParametersForHisState()
    {
        var valueObjects = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(ValueObject)).GetTypes();
        var failingTypes = new List<Type>();
        foreach (var entityType in valueObjects)
        {
            bool hasExpectedConstructor = false;

            const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                              BindingFlags.Public |
                                              BindingFlags.Instance;
            var names = entityType.GetFields(bindingFlags).Select(x => x.Name.ToUpperInvariant()).ToList();
            var propertyNames = entityType.GetProperties(bindingFlags).Select(x => x.Name.ToUpperInvariant()).ToList();
            names.AddRange(propertyNames);
            var constructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var constructorInfo in constructors)
            {
                var parameters = constructorInfo.GetParameters().Select(x => x.Name?.ToUpperInvariant()).ToList();

                if (names.Intersect(parameters).Count() == names.Count)
                {
                    hasExpectedConstructor = true;
                    break;
                }
            }
            if (!hasExpectedConstructor)
            {
                failingTypes.Add(entityType);
            }
        }
        AssertFailingTypes(failingTypes);
    }
}
