using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using MediatR;
using NetArchTest.Rules;

namespace HC.LIS.Modules.TestOrders.ArchTests.Application;

public class ApplicationTests : TestBase
{
    [Fact]
    public void CommandShouldBeImmutable()
    {
        IEnumerable<Type> types = Types.InAssembly(ApplicationAssembly)
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
        IEnumerable<Type> types = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(IQuery<>)).GetTypes();

        AssertAreImmutable(types);
    }

    [Fact]
    public void CommandHandlerShouldHaveNameEndingWithCommandHandler()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
          .That()
          .ImplementInterface(typeof(ICommandHandler<>))
              .Or()
          .ImplementInterface(typeof(ICommandHandler<,>))
          .And()
          .DoNotHaveNameMatching(".*Decorator.*").Should()
          .HaveNameEndingWith("CommandHandler", System.StringComparison.OrdinalIgnoreCase)
          .GetResult();

        AssertArchTestResult(result);
    }

    [Fact]
    public void QueryHandlerShouldHaveNameEndingWithQueryHandler()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler", System.StringComparison.OrdinalIgnoreCase)
            .GetResult();

        AssertArchTestResult(result);
    }

    [Fact]
    public void CommandAndQueryHandlersShouldNotBePublic()
    {
        IReadOnlyList<Type> types = Types.InAssembly(ApplicationAssembly)
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
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator", System.StringComparison.OrdinalIgnoreCase)
            .GetResult();

        AssertArchTestResult(result);
    }

    [Fact]
    public void ValidatorsShouldNotBePublic()
    {
        IReadOnlyList<Type> types = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should().NotBePublic().GetResult().FailingTypes;

        AssertFailingTypes(types);
    }

    [Fact]
    public void InternalCommandShouldHaveConstructorWithJsonConstructorAttribute()
    {
        IEnumerable<Type> types = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(InternalCommandBase))
            .Or()
            .Inherit(typeof(InternalCommandBase<>))
            .GetTypes();

        var failingTypes = new List<Type>();

        foreach (Type? type in types)
        {
            bool hasJsonConstructorDefined = false;
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
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
        IEnumerable<Type> types = Types.InAssembly(ApplicationAssembly)
            .That().DoNotHaveName("ICommandHandler`1")
            .Should().ImplementInterface(typeof(IRequestHandler<>))
            .GetTypes();

        List<Type> failingTypes = [];
        foreach (Type type in types)
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
