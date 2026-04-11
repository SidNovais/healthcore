# Template: ModuleTests.cs

This file must be created at `src/HC.LIS/HC.LIS.ArchTests/Modules/ModuleTests.cs`.

**Purpose:** One `[Fact]` per module asserting that the module's 3 assemblies have no dependency on any other module's namespace. These tests enforce the module isolation boundary.

**Namespace:** `HC.LIS.ArchTests.Modules`

**Class:** `public class ModuleTests : TestBase`

**Required usings:**
- `System` (for `StringComparison`)
- `System.Collections.Generic` (for `List<>`)
- `System.Reflection` (for `Assembly`)
- `MediatR` (for `INotificationHandler<>`)
- `NetArchTest.Rules`

> Note: This project uses `Microsoft.NET.Sdk` and `TreatWarningsAsErrors=true` with `AnalysisMode=All`. Implicit usings are NOT sufficient — always add `using System;`, `using System.Collections.Generic;`, and `using System.Reflection;` explicitly.

**Test method naming:** Use PascalCase (no underscores) — CA1707 is a hard error. Pattern: `{ModuleName}DoesNotHaveDependencyOnOtherModules`.

**Test method pattern** (replicate for each discovered module):

```csharp
[Fact]
public void {ModuleName}DoesNotHaveDependencyOnOtherModules()
{
    List<string> otherModules =
    [
        // all module namespace constants EXCEPT the one being tested
        {OtherModule1}Namespace,
        {OtherModule2}Namespace,
        {OtherModule3}Namespace
    ];
    List<Assembly> assemblies =
    [
        {ModuleName}Domain,
        {ModuleName}Application,
        {ModuleName}Infrastructure
    ];
    var result = Types.InAssemblies(assemblies)
        .That()
            .DoNotImplementInterface(typeof(INotificationHandler<>))
            .And().DoNotHaveNameEndingWith("IntegrationEventHandler")
            .And().DoNotHaveName("EventsBusStartup")
        .Should()
        .NotHaveDependencyOnAny([.. otherModules])
        .GetResult();
    AssertArchTestResult(result);
}
```

**Exclusion rationale:**
- `DoNotImplementInterface(typeof(INotificationHandler<>))` — integration event handlers legitimately subscribe to events from other modules; excluding them from the check avoids false positives
- `DoNotHaveNameEndingWith("IntegrationEventHandler")` — belt-and-suspenders for handler naming convention
- `DoNotHaveName("EventsBusStartup")` — startup wiring class may reference other assemblies for bootstrapping

**otherModules rule:** For each module, the `otherModules` list contains ALL other module namespace constants — exactly N-1 entries for an N-module system. Never omit a module from another module's list.

**Generated file example** (4 modules):

```csharp
using System.Reflection;
using MediatR;
using NetArchTest.Rules;

namespace HC.LIS.ArchTests.Modules;

public class ModuleTests : TestBase
{
    [Fact]
    public void TestOrders_DoesNotHave_Dependency_On_Other_Modules()
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
    public void Analyzer_DoesNotHave_Dependency_On_Other_Modules()
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
    public void SampleCollection_DoesNotHave_Dependency_On_Other_Modules()
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
    public void LabAnalysis_DoesNotHave_Dependency_On_Other_Modules()
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
```
