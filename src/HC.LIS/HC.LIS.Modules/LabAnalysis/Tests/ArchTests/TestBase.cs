using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Infrastructure;
using NetArchTest.Rules;
using HC.LIS.Modules.LabAnalysis.Domain;

namespace HC.LIS.Modules.LabAnalysis.ArchTests;

public abstract class TestBase
{
    protected static Assembly InfrastructureAssembly => typeof(LabAnalysisModule).Assembly;
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
