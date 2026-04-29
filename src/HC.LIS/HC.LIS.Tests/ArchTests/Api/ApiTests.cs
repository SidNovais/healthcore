using System.Collections.Generic;
using NetArchTest.Rules;

namespace HC.LIS.ArchTests.Api;

public class ApiTests : TestBase
{
    [Fact]
    public void TestOrdersApiDoesNotHaveDependencyToOtherModules()
    {
        List<string> otherModules =
        [
            AnalyzerNamespace,
            SampleCollectionNamespace,
            LabAnalysisNamespace
        ];
        var result = Types.InAssembly(ApiAssembly)
            .That()
                .ResideInNamespace("HC.LIS.API.Modules.TestOrders")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void AnalyzerApiDoesNotHaveDependencyToOtherModules()
    {
        List<string> otherModules =
        [
            TestOrdersNamespace,
            SampleCollectionNamespace,
            LabAnalysisNamespace
        ];
        var result = Types.InAssembly(ApiAssembly)
            .That()
                .ResideInNamespace("HC.LIS.API.Modules.Analyzer")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void SampleCollectionApiDoesNotHaveDependencyToOtherModules()
    {
        List<string> otherModules =
        [
            TestOrdersNamespace,
            AnalyzerNamespace,
            LabAnalysisNamespace
        ];
        var result = Types.InAssembly(ApiAssembly)
            .That()
                .ResideInNamespace("HC.LIS.API.Modules.SampleCollection")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }

    [Fact]
    public void LabAnalysisApiDoesNotHaveDependencyToOtherModules()
    {
        List<string> otherModules =
        [
            TestOrdersNamespace,
            AnalyzerNamespace,
            SampleCollectionNamespace
        ];
        var result = Types.InAssembly(ApiAssembly)
            .That()
                .ResideInNamespace("HC.LIS.API.Modules.LabAnalysis")
            .Should()
            .NotHaveDependencyOnAny([.. otherModules])
            .GetResult();
        AssertArchTestResult(result);
    }
}
