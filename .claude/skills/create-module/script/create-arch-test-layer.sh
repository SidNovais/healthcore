#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-arch-test-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}/Tests
MODULE_DIR=$BASE_MODULE_DIR/ArchTests/Module
DOMAIN_DIR=$BASE_MODULE_DIR/ArchTests/Domain
APPLICATION_DIR=$BASE_MODULE_DIR/ArchTests/Application

mkdir -p ${BASE_MODULE_DIR}/ArchTests

cat > "${BASE_MODULE_DIR}/ArchTests/${ROOT_NS}.Modules.${MODULE_NAME}.ArchTests.csproj" << 'CSPROJEOF'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NetArchTest.Rules" />
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

mkdir -p $MODULE_DIR
mkdir -p $DOMAIN_DIR
mkdir -p $APPLICATION_DIR

cat > "${BASE_MODULE_DIR}/ArchTests/TestBase.cs" << EOF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure;
using NetArchTest.Rules;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Domain;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.ArchTests;

public abstract class TestBase
{
    protected static Assembly InfrastructureAssembly => typeof(${MODULE_NAME}Module).Assembly;
    protected static Assembly ApplicationAssembly => typeof(ICommand).Assembly;
    protected static Assembly DomainAssembly => typeof(DomainAssemblyInfo).Assembly;

    protected static void AssertAreImmutable(IEnumerable<Type> types)
    {
        IList<Type> failingTypes = [];
        foreach (var type in types)
        {
            if (type.GetFields().Any(x => !x.IsInitOnly) || type.GetProperties().Any(x => x.CanWrite))
            {
                failingTypes.Add(type);
                break;
            }
        }
        AssertFailingTypes(failingTypes);
    }

    protected static void AssertFailingTypes(IEnumerable<Type> types)
    {
        types.Should().BeNullOrEmpty();
    }

    protected static void AssertArchTestResult(TestResult result)
    {
        AssertFailingTypes(result.FailingTypes);
    }
}
EOF

cat > "${MODULE_DIR}/LayersTests.cs" << EOF
using NetArchTest.Rules;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.ArchTests.Layers;

public class LayersTests : TestBase
{
    [Fact]
    public void DomainLayerShouldNotHaveDependencyToApplicationLayer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void DomainLayerShouldNotHaveDependencyToInfrastructureLayer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void ApplicationLayerShouldNotHaveDependencyToInfrastructureLayer()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();
        AssertArchTestResult(result);
    }
}
EOF

cat > "${DOMAIN_DIR}/DomainTests.cs" << EOF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using HC.Core.Domain;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.ArchTests.Domain;

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
EOF

cat > "${APPLICATION_DIR}/ApplicationTests.cs" << EOF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using MediatR;
using NetArchTest.Rules;
using Newtonsoft.Json;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Queries;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.ArchTests.Application;

public class ApplicationTests : TestBase
{
    [Fact]
    public void CommandShouldBeImmutable()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(CommandBase))
            .Or()
            .Inherit(typeof(CommandBase<>))
            .Or()
            .Inherit(typeof(InternalCommandBase))
            .Or()
            .Inherit(typeof(InternalCommandBase<>))
            .Or()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .GetTypes();

        AssertAreImmutable(types);
    }

    [Fact]
    public void QueryShouldBeImmutable()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(IQuery<>)).GetTypes();

        AssertAreImmutable(types);
    }

    [Fact]
    public void CommandHandlerShouldHaveNameEndingWithCommandHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
                .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And()
            .DoNotHaveNameMatching(".*Decorator.*").Should()
            .HaveNameEndingWith("CommandHandler", StringComparison.OrdinalIgnoreCase)
            .GetResult();

        AssertArchTestResult(result);
    }

    [Fact]
    public void QueryHandlerShouldHaveNameEndingWithQueryHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler", StringComparison.OrdinalIgnoreCase)
            .GetResult();

        AssertArchTestResult(result);
    }

    [Fact]
    public void CommandAndQueryHandlersShouldNotBePublic()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That()
                .ImplementInterface(typeof(IQueryHandler<,>))
                    .Or()
                .ImplementInterface(typeof(ICommandHandler<>))
                    .Or()
                .ImplementInterface(typeof(ICommandHandler<,>))
            .Should().NotBePublic().GetResult().FailingTypes;

        AssertFailingTypes(types);
    }

    [Fact]
    public void ValidatorShouldHaveNameEndingWithValidator()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator", StringComparison.OrdinalIgnoreCase)
            .GetResult();

        AssertArchTestResult(result);
    }

    [Fact]
    public void ValidatorsShouldNotBePublic()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should().NotBePublic().GetResult().FailingTypes;

        AssertFailingTypes(types);
    }

    [Fact]
    public void InternalCommandShouldHaveConstructorWithJsonConstructorAttribute()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(InternalCommandBase))
            .Or()
            .Inherit(typeof(InternalCommandBase<>))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (var type in types)
        {
            bool hasJsonConstructorDefined = false;
            var constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var constructorInfo in constructors)
            {
                var jsonConstructorAttribute = constructorInfo.GetCustomAttributes(typeof(JsonConstructorAttribute), false);
                if (jsonConstructorAttribute.Length > 0)
                {
                    hasJsonConstructorDefined = true;
                    break;
                }
            }

            if (!hasJsonConstructorDefined)
            {
                failingTypes.Add(type);
            }
        }

        AssertFailingTypes(failingTypes);
    }

    [Fact]
    public void MediatRRequestHandlerShouldNotBeUsedDirectly()
    {
        var types = Types.InAssembly(ApplicationAssembly)
            .That().DoNotHaveName("ICommandHandler\`1")
            .Should().ImplementInterface(typeof(IRequestHandler<>))
            .GetTypes();

        List<Type> failingTypes = [];
        foreach (var type in types)
        {
            bool isCommandHandler = type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
            bool isCommandWithResultHandler = type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
            bool isQueryHandler = type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
            if (!isCommandHandler && !isCommandWithResultHandler && !isQueryHandler)
            {
                failingTypes.Add(type);
            }
        }

        AssertFailingTypes(failingTypes);
    }

    [Fact]
    public void CommandWithResultShouldNotReturnUnit()
    {
        Type commandWithResultHandlerType = typeof(ICommandHandler<,>);
        IEnumerable<Type> types = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(commandWithResultHandlerType)
            .GetTypes().ToList();

        var failingTypes = new List<Type>();
        foreach (Type type in types)
        {
            Type? interfaceType = type.GetInterface(commandWithResultHandlerType.Name);
            if (interfaceType?.GenericTypeArguments[1] == typeof(Unit))
            {
                failingTypes.Add(type);
            }
        }

        AssertFailingTypes(failingTypes);
    }
}
EOF
