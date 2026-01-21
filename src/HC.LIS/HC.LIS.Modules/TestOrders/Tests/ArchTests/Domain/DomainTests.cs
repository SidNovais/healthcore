using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.Core.Application.Events;
using HC.Core.Domain;
using NetArchTest.Rules;

namespace HC.LIS.Modules.TestOrders.ArchTests.Domain;

public class DomainTests : TestBase
{
    [Fact]
    public void DomainEventShouldBeImmutable()
    {
        IEnumerable<Type> types = Types.InAssembly(DomainAssembly)
            .That()
              .Inherit(typeof(DomainEventBase))
                .Or()
              .ImplementInterface(typeof(IDomainEvent))
            .GetTypes();
        AssertAreImmutable(types);
    }

    [Fact]
    public void ValueObjectShouldBeImmutable()
    {
        IEnumerable<Type> types = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(ValueObject))
            .GetTypes();
        AssertAreImmutable(types);
    }

    [Fact]
    public void BusinessRuleShouldHaveRulePostfix()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IBusinessRule))
            .Should().HaveNameEndingWith("Rule", StringComparison.OrdinalIgnoreCase)
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void DomainEventShouldHaveDomainEventPostfix()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Or()
            .Inherit(typeof(DomainEventBase))
            .Should().HaveNameEndingWith("DomainEvent", StringComparison.OrdinalIgnoreCase)
            .GetResult();
        AssertArchTestResult(result);
    }
    [Fact]
    public void EntityShouldNotHavePublicMembersWhenIsNotAggregateRoot()
    {
        IEnumerable<Type> types = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .And().DoNotImplementInterface(typeof(IAggregateRoot)).GetTypes();

        const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                          BindingFlags.Public |
                                          BindingFlags.Instance |
                                          BindingFlags.Static;

        var failingTypes = new List<Type>();
        foreach (Type type in types)
        {
            FieldInfo[] publicFields = type.GetFields(bindingFlags);
            PropertyInfo[] publicProperties = type.GetProperties(bindingFlags);
            MethodInfo[] publicMethods = type.GetMethods(bindingFlags);

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
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity)).GetTypes();
        var aggregateRoots = Types.InAssembly(DomainAssembly)
            .That().ImplementInterface(typeof(IAggregateRoot)).GetTypes().ToList();
        const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                          BindingFlags.NonPublic |
                                          BindingFlags.Instance;
        var failingTypes = new List<Type>();
        foreach (Type type in entityTypes)
        {
            FieldInfo[] fields = type.GetFields(bindingFlags);

            foreach (FieldInfo field in fields)
            {
                if (aggregateRoots.Contains(field.FieldType) ||
                    field.FieldType.GenericTypeArguments.Any(x => aggregateRoots.Contains(x)))
                {
                    failingTypes.Add(type);
                    break;
                }
            }

            PropertyInfo[] properties = type.GetProperties(bindingFlags);
            foreach (PropertyInfo property in properties)
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
    public void EntityShouldHaveParameterlessInPrivateConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity)).GetTypes();
        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            bool hasPrivateParameterlessConstructor = false;
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (ConstructorInfo constructorInfo in constructors)
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
        IEnumerable<Type> domainObjectTypes = Types.InAssembly(DomainAssembly)
            .That()
                    .Inherit(typeof(Entity))
                .Or()
                    .Inherit(typeof(ValueObject))
            .GetTypes();
        var failingTypes = new List<Type>();
        foreach (Type domainObjectType in domainObjectTypes)
        {
            ConstructorInfo[] constructors = domainObjectType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (ConstructorInfo constructorInfo in constructors)
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
        IEnumerable<Type> valueObjects = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(ValueObject)).GetTypes();
        var failingTypes = new List<Type>();
        foreach (Type entityType in valueObjects)
        {
            bool hasExpectedConstructor = false;

            const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                                BindingFlags.Public |
                                                BindingFlags.Instance;
            var names = entityType.GetFields(bindingFlags).Select(x => x.Name.ToUpperInvariant()).ToList();
            var propertyNames = entityType.GetProperties(bindingFlags).Select(x => x.Name.ToUpperInvariant()).ToList();
            names.AddRange(propertyNames);
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (ConstructorInfo constructorInfo in constructors)
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
