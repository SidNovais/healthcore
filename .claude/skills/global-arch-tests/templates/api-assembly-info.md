# Template: ApiAssemblyInfo.cs

This file must be created in `src/HC.LIS/HC.LIS.API/ApiAssemblyInfo.cs`.

**Purpose:** All types in `HC.LIS.API` are `internal`. Without a public anchor type, the test project cannot call `typeof(X).Assembly` to reference the API assembly. This empty marker class provides that anchor.

**Generated file:**

```csharp
namespace HC.LIS.API;

public class ApiAssemblyInfo { }
```

**Rules:**
- Namespace must be `HC.LIS.API` (matches the API project's `RootNamespace`)
- The class must be `public` — this is the entire purpose of the file
- No logic, no members, no base class
- Use file-scoped namespace (no braces)
