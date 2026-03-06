using NetArchTest.Rules;

namespace HC.LIS.Modules.LabAnalysis.ArchTests.Layers;

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
