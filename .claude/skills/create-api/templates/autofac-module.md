# Template: {ModuleName}AutofacModule.cs

**Output path:** `src/HC.LIS/HC.LIS.API/Modules/{ModuleName}/{ModuleName}AutofacModule.cs`

Generate one file per selected module. Replace `{ModuleName}` with the actual module name.

```csharp
using Autofac;
using HC.LIS.Modules.{ModuleName}.Application.Contracts;
using HC.LIS.Modules.{ModuleName}.Infrastructure;

namespace HC.LIS.API.Modules.{ModuleName};

internal sealed class {ModuleName}AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<{ModuleName}Module>()
            .As<I{ModuleName}Module>()
            .InstancePerLifetimeScope();
    }
}
```

**Important:** Before writing this file, confirm the actual namespace and class names by reading:
- `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Application/Contracts/I{ModuleName}Module.cs`
- `src/HC.LIS/HC.LIS.Modules/{ModuleName}/Infrastructure/{ModuleName}Module.cs`

The module implementation class is the concrete class that implements the facade interface.
