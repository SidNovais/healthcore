# Template: ApiTests.cs

This file must be created at `src/HC.LIS/HC.LIS.ArchTests/Api/ApiTests.cs`.

**Purpose:** One `[Fact]` per module asserting that the module's endpoint namespace in `HC.LIS.API` has no dependency on any other module's namespace. These tests enforce API-level isolation — endpoint handlers for module A must not reference types from module B.

**Namespace:** `HC.LIS.ArchTests.Api`

**Class:** `public class ApiTests : TestBase`

**Required usings:**
- `System.Collections.Generic` (for `List<>`)
- `NetArchTest.Rules`

> Note: `TreatWarningsAsErrors=true` with `AnalysisMode=All` makes CA1707 a hard error. Test method names must be PascalCase (no underscores). Add `using System.Collections.Generic;` explicitly — implicit usings are not reliable in this project.

**Test method naming:** PascalCase, no underscores — CA1707 is a hard error. Pattern: `{ModuleName}ApiDoesNotHaveDependencyToOtherModules`.

**Test method pattern** (replicate for each discovered module):

```csharp
[Fact]
public void {ModuleName}ApiDoesNotHaveDependencyToOtherModules()
{
    List<string> otherModules =
    [
        // all module namespace constants EXCEPT the one being tested
        {OtherModule1}Namespace,
        {OtherModule2}Namespace,
        {OtherModule3}Namespace
    ];
    var result = Types.InAssembly(ApiAssembly)
        .That()
            .ResideInNamespace("HC.LIS.API.Modules.{ModuleName}")
        .Should()
        .NotHaveDependencyOnAny([.. otherModules])
        .GetResult();
    AssertArchTestResult(result);
}
```

**Key differences from ModuleTests:**
- Uses `Types.InAssembly(ApiAssembly)` (single assembly), not `Types.InAssemblies(list)`
- Filters to a specific namespace via `.ResideInNamespace("HC.LIS.API.Modules.{ModuleName}")`
- No `INotificationHandler<>` exclusion — API endpoint classes must not be MediatR handlers
- No `EventsBusStartup` exclusion — that class belongs to module Infrastructure, not the API

**API namespace pattern:** `HC.LIS.API.Modules.{ModuleName}` — confirmed from the endpoint files, e.g.:
- `HC.LIS.API.Modules.TestOrders.Orders` is under `HC.LIS.API.Modules.TestOrders`
- `HC.LIS.API.Modules.Analyzer.AnalyzerSamples` is under `HC.LIS.API.Modules.Analyzer`
- `ResideInNamespace` matches types in the given namespace AND all sub-namespaces

**Generated file example** (4 modules):

```csharp
using NetArchTest.Rules;

namespace HC.LIS.ArchTests.Api;

public class ApiTests : TestBase
{
    [Fact]
    public void TestOrdersApi_DoesNotHaveDependency_ToOtherModules()
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
    public void AnalyzerApi_DoesNotHaveDependency_ToOtherModules()
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
    public void SampleCollectionApi_DoesNotHaveDependency_ToOtherModules()
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
    public void LabAnalysisApi_DoesNotHaveDependency_ToOtherModules()
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
```
