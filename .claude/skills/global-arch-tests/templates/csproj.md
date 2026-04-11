# Template: HC.LIS.ArchTests.csproj

This file must be created at `src/HC.LIS/HC.LIS.ArchTests/HC.LIS.ArchTests.csproj`.

**Important:** The `Directory.Build.targets` auto-link rule for `.ArchTests` projects adds `..\..\Infrastructure\*.csproj`. For a project at `src/HC.LIS/HC.LIS.ArchTests/`, that resolves to `src/HC.LIS/Infrastructure/*.csproj` — which does not exist. The auto-link produces nothing, so an explicit `<ProjectReference>` is required.

`HC.LIS.API.csproj` already directly references all 4 module Infrastructure projects. Through transitive references, a single reference to `HC.LIS.API` gives access to all module Domain, Application, and Infrastructure assemblies. No per-module references are needed.

**Do NOT specify `<TargetFramework>` or `<Nullable>` or `<ImplicitUsings>` — these are inherited from `Directory.Build.props`.**

**No package versions** — versions come from `Directory.Packages.props` (central package management is enabled).

**Generated file:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions"/>
    <PackageReference Include="Microsoft.NET.Test.Sdk"/>
    <PackageReference Include="NetArchTest.Rules"/>
    <PackageReference Include="xunit"/>
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <!-- The Directory.Build.targets auto-link for .ArchTests resolves to a nonexistent path
         for this project location. HC.LIS.API already references all module Infrastructure
         projects transitively, so one explicit reference is sufficient. -->
    <ProjectReference Include="..\HC.LIS.API\HC.LIS.API.csproj" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```
