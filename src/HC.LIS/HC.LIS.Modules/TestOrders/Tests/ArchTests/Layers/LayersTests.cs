using HC.LIS.Modules.TestOrders.ArchTests;
using NetArchTest.Rules;

namespace HC.LIS.Modules.TestOrders.ArchTests.Layers;

public class LayersTests : TestBase
{
    [Fact]
    public void DomainLayerShouldNotHaveDependencyToApplicationLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult()
        ;
        AssertArchTestResult(result);
    }

    [Fact]
    public void DomainLayerShouldNotHaveDependencyToInfrastructureLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
        ;
        AssertArchTestResult(result);
    }

    [Fact]
    public void ApplicationLayerShouldNotHaveDependencyToInfrastructureLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
        ;
        AssertArchTestResult(result);
    }
}
