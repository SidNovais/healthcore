# Template: HC.LIS.API.csproj

**Output path:** `src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj`

**Placeholders:**
- `__ModuleProjectRefs__` — one `<ProjectReference>` per selected module's Infrastructure project

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <RootNamespace>HC.LIS.API</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Autofac.Extensions.DependencyInjection" Version="*" />
    <PackageReference Include="Asp.Versioning.Http" Version="*" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="*" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="*" />
    <PackageReference Include="Serilog.AspNetCore" Version="*" />
    <PackageReference Include="Serilog.Sinks.Console" Version="*" />
  </ItemGroup>

  <ItemGroup>
    __ModuleProjectRefs__
    <ProjectReference Include="..\..\HC.Core\Application\HC.Core.Application.csproj" />
    <ProjectReference Include="..\..\HC.Core\Domain\HC.Core.Domain.csproj" />
  </ItemGroup>
</Project>
```

**`__ModuleProjectRefs__` pattern** (one entry per selected module):
```xml
<ProjectReference Include="..\HC.LIS.Modules\{ModuleName}\Infrastructure\HC.LIS.Modules.{ModuleName}.Infrastructure.csproj" />
```
