using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR;
using NetArchTest.Rules;

namespace HC.LIS.ArchTests.Modules;

public class ModuleTests : TestBase
{
    [Fact]
    public void TestOrdersDoesNotHaveDependencyOnOtherModules()
    {
        List<string> otherModules =
        [
            AnalyzerNamespace,
            SampleCollectionNamespace,
            LabAnalysisNamespace
        ];
        List<Assembly> assemblies =
        [
            TestOrdersDomain,
            TestOrdersApplication,
            TestOrdersInfrastructure
        ];
        var result = Types.InAssemblies(assemblies)
            .That()
                .DoNotImplementInterface(typeof(INotificationHandler<>))
                .And().DoNotHaveNameEndingWith("IntegrationEventHandler", StringComparison.OrdinalIgnoreCase)
                .And().DoNotHaveName("EventsBusStartup")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void AnalyzerDoesNotHaveDependencyOnOtherModules()
    {
        List<string> otherModules =
        [
            TestOrdersNamespace,
            SampleCollectionNamespace,
            LabAnalysisNamespace
        ];
        List<Assembly> assemblies =
        [
            AnalyzerDomain,
            AnalyzerApplication,
            AnalyzerInfrastructure
        ];
        var result = Types.InAssemblies(assemblies)
            .That()
                .DoNotImplementInterface(typeof(INotificationHandler<>))
                .And().DoNotHaveNameEndingWith("IntegrationEventHandler", StringComparison.OrdinalIgnoreCase)
                .And().DoNotHaveName("EventsBusStartup")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void SampleCollectionDoesNotHaveDependencyOnOtherModules()
    {
        List<string> otherModules =
        [
            TestOrdersNamespace,
            AnalyzerNamespace,
            LabAnalysisNamespace
        ];
        List<Assembly> assemblies =
        [
            SampleCollectionDomain,
            SampleCollectionApplication,
            SampleCollectionInfrastructure
        ];
        var result = Types.InAssemblies(assemblies)
            .That()
                .DoNotImplementInterface(typeof(INotificationHandler<>))
                .And().DoNotHaveNameEndingWith("IntegrationEventHandler", StringComparison.OrdinalIgnoreCase)
                .And().DoNotHaveName("EventsBusStartup")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void LabAnalysisDoesNotHaveDependencyOnOtherModules()
    {
        List<string> otherModules =
        [
            TestOrdersNamespace,
            AnalyzerNamespace,
            SampleCollectionNamespace
        ];
        List<Assembly> assemblies =
        [
            LabAnalysisDomain,
            LabAnalysisApplication,
            LabAnalysisInfrastructure
        ];
        var result = Types.InAssemblies(assemblies)
            .That()
                .DoNotImplementInterface(typeof(INotificationHandler<>))
                .And().DoNotHaveNameEndingWith("IntegrationEventHandler", StringComparison.OrdinalIgnoreCase)
                .And().DoNotHaveName("EventsBusStartup")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }
}
