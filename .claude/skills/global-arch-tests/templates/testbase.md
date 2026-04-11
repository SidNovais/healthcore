# Template: TestBase.cs

This file must be created at `src/HC.LIS/HC.LIS.ArchTests/TestBase.cs`.

**Purpose:** Shared infrastructure for all global arch tests. Provides:
- API assembly anchor (`ApiAssembly`)
- Module namespace string constants (used as arguments to `NotHaveDependencyOnAny`)
- Per-module assembly anchor properties (Domain, Application, Infrastructure)
- Assert helpers (`AssertAreImmutable`, `AssertFailingTypes`, `AssertArchTestResult`)

**DomainAssemblyInfo disambiguation:** Every module has a class named `DomainAssemblyInfo` in its own namespace. C# cannot resolve `typeof(DomainAssemblyInfo)` without disambiguation, so `using` type aliases are required — one per module.

**Namespace:** `HC.LIS.ArchTests`

**Use file-scoped namespaces (no braces).**

**Assembly anchor types per module:**
- Domain: aliased `DomainAssemblyInfo` (e.g., `typeof(TestOrdersDomainAssembly).Assembly`)
- Application: `I{Module}Module` interface (e.g., `typeof(ITestOrdersModule).Assembly`)
- Infrastructure: `{Module}Module` class (e.g., `typeof(TestOrdersModule).Assembly`)

**Module namespace constants** follow the pattern `"HC.LIS.Modules.{ModuleName}"`.

**ApiAssembly** uses `typeof(ApiAssemblyInfo).Assembly` — the marker class added to `HC.LIS.API`.

**Generated file structure** (adapt `using` aliases, constants, and assembly anchors for each discovered module):

```csharp
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
using TestOrdersDomainAssembly = HC.LIS.Modules.TestOrders.Domain.DomainAssemblyInfo;
using AnalyzerDomainAssembly = HC.LIS.Modules.Analyzer.Domain.DomainAssemblyInfo;
using SampleCollectionDomainAssembly = HC.LIS.Modules.SampleCollection.Domain.DomainAssemblyInfo;
using LabAnalysisDomainAssembly = HC.LIS.Modules.LabAnalysis.Domain.DomainAssemblyInfo;

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
```
