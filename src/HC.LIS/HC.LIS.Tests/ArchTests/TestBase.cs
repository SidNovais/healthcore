using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using HC.LIS.API;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Infrastructure;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.Infrastructure;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Infrastructure;
using NetArchTest.Rules;
using AnalyzerDomainAssembly = HC.LIS.Modules.Analyzer.Domain.DomainAssemblyInfo;
using LabAnalysisDomainAssembly = HC.LIS.Modules.LabAnalysis.Domain.DomainAssemblyInfo;
using SampleCollectionDomainAssembly = HC.LIS.Modules.SampleCollection.Domain.DomainAssemblyInfo;
using TestOrdersDomainAssembly = HC.LIS.Modules.TestOrders.Domain.DomainAssemblyInfo;

namespace HC.LIS.ArchTests;

public abstract class TestBase
{
    protected static Assembly ApiAssembly => typeof(ApiAssemblyInfo).Assembly;

    protected const string TestOrdersNamespace = "HC.LIS.Modules.TestOrders";
    protected const string AnalyzerNamespace = "HC.LIS.Modules.Analyzer";
    protected const string SampleCollectionNamespace = "HC.LIS.Modules.SampleCollection";
    protected const string LabAnalysisNamespace = "HC.LIS.Modules.LabAnalysis";

    protected static Assembly TestOrdersDomain => typeof(TestOrdersDomainAssembly).Assembly;
    protected static Assembly TestOrdersApplication => typeof(ITestOrdersModule).Assembly;
    protected static Assembly TestOrdersInfrastructure => typeof(TestOrdersModule).Assembly;

    protected static Assembly AnalyzerDomain => typeof(AnalyzerDomainAssembly).Assembly;
    protected static Assembly AnalyzerApplication => typeof(IAnalyzerModule).Assembly;
    protected static Assembly AnalyzerInfrastructure => typeof(AnalyzerModule).Assembly;

    protected static Assembly SampleCollectionDomain => typeof(SampleCollectionDomainAssembly).Assembly;
    protected static Assembly SampleCollectionApplication => typeof(ISampleCollectionModule).Assembly;
    protected static Assembly SampleCollectionInfrastructure => typeof(SampleCollectionModule).Assembly;

    protected static Assembly LabAnalysisDomain => typeof(LabAnalysisDomainAssembly).Assembly;
    protected static Assembly LabAnalysisApplication => typeof(ILabAnalysisModule).Assembly;
    protected static Assembly LabAnalysisInfrastructure => typeof(LabAnalysisModule).Assembly;

    protected static void AssertAreImmutable(IEnumerable<Type> types)
    {
        IList<Type> failingTypes = [];
        foreach (Type type in types)
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
        => types.Should().BeNullOrEmpty();

    protected static void AssertArchTestResult(TestResult result)
        => AssertFailingTypes(result.FailingTypes);
}
