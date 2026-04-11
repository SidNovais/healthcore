# Template: add-module mode

Use this guide when `src/HC.LIS/HC.LIS.ArchTests/` already exists and the task is to add tests for a new module.

## Step 0 — Verify the new module's anchor types

Before editing any file, confirm that these 3 files exist for the new module:
- `src/HC.LIS/HC.LIS.Modules/{NewModule}/Domain/DomainAssemblyInfo.cs`
- `src/HC.LIS/HC.LIS.Modules/{NewModule}/Application/Contracts/I{NewModule}Module.cs`
- `src/HC.LIS/HC.LIS.Modules/{NewModule}/Infrastructure/{NewModule}Module.cs`

If any are missing, stop and ask the user to scaffold the module first.

---

## Step 1 — Edit `TestBase.cs`

**1a.** Add a `using` alias at the top of the file (keep alias usings sorted with other alias usings):
```csharp
using {NewModule}DomainAssembly = HC.LIS.Modules.{NewModule}.Domain.DomainAssemblyInfo;
```

**1b.** Add standard `using` directives for the new module's Application.Contracts and Infrastructure namespaces:
```csharp
using HC.LIS.Modules.{NewModule}.Application.Contracts;
using HC.LIS.Modules.{NewModule}.Infrastructure;
```

**1c.** Add a namespace constant (keep constants together, alphabetically or in declaration order):
```csharp
protected const string {NewModule}Namespace = "HC.LIS.Modules.{NewModule}";
```

**1d.** Add three assembly anchor properties (keep grouped with other module anchor blocks):
```csharp
protected static Assembly {NewModule}Domain => typeof({NewModule}DomainAssembly).Assembly;
protected static Assembly {NewModule}Application => typeof(I{NewModule}Module).Assembly;
protected static Assembly {NewModule}Infrastructure => typeof({NewModule}Module).Assembly;
```

---

## Step 2 — Edit `Modules/ModuleTests.cs`

**2a. Add a new `[Fact]` method** at the end of the `ModuleTests` class:
```csharp
[Fact]
public void {NewModule}_DoesNotHave_Dependency_On_Other_Modules()
{
    List<string> otherModules =
    [
        // All existing module namespace constants
        {ExistingModule1}Namespace,
        {ExistingModule2}Namespace,
        // ...
    ];
    List<Assembly> assemblies =
    [
        {NewModule}Domain,
        {NewModule}Application,
        {NewModule}Infrastructure
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

**2b. CRITICAL — Update ALL existing `otherModules` lists** in `ModuleTests.cs`:

Read the entire file and find every `List<string> otherModules = [` block. Add `{NewModule}Namespace,` to each one. Do NOT skip a single test — missing even one means that test no longer enforces isolation against the new module.

Example: if `TestOrders_DoesNotHave_Dependency_On_Other_Modules` previously had:
```csharp
List<string> otherModules =
[
    AnalyzerNamespace,
    SampleCollectionNamespace,
    LabAnalysisNamespace
];
```

After adding `PatientManagement` module it must become:
```csharp
List<string> otherModules =
[
    AnalyzerNamespace,
    SampleCollectionNamespace,
    LabAnalysisNamespace,
    PatientManagementNamespace
];
```

---

## Step 3 — Edit `Api/ApiTests.cs`

**3a. Add a new `[Fact]` method** at the end of the `ApiTests` class:
```csharp
[Fact]
public void {NewModule}Api_DoesNotHaveDependency_ToOtherModules()
{
    List<string> otherModules =
    [
        // All existing module namespace constants
        {ExistingModule1}Namespace,
        {ExistingModule2}Namespace,
        // ...
    ];
    var result = Types.InAssembly(ApiAssembly)
        .That()
            .ResideInNamespace("HC.LIS.API.Modules.{NewModule}")
        .Should()
        .NotHaveDependencyOnAny([.. otherModules])
        .GetResult();
    AssertArchTestResult(result);
}
```

**3b. CRITICAL — Update ALL existing `otherModules` lists** in `ApiTests.cs`:

Same rule as for `ModuleTests.cs` — every existing test's `otherModules` list must gain the new module's namespace constant.

---

## Step 4 — Verify

```bash
dotnet build src/HC.LIS/HC.LIS.ArchTests/HC.LIS.ArchTests.csproj
dotnet test src/HC.LIS/HC.LIS.ArchTests/HC.LIS.ArchTests.csproj --no-build
```

After adding one module: expect 2 more tests (1 in ModuleTests + 1 in ApiTests) and all `otherModules` lists to be one entry longer than before.
