# Template: ExceptionHandlerExtensions.cs

**Output path:** `src/HC.LIS/HC.LIS.API/Configuration/Validation/ExceptionHandlerExtensions.cs`

Works the same for Minimal APIs as for MVC — registered once in `Program.cs` before endpoint mapping.

```csharp
using HC.Core.Application;
using HC.Core.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HC.LIS.API.Configuration.Validation;

internal static class ExceptionHandlerExtensions
{
    internal static IApplicationBuilder UseHcLisExceptionHandler(
        this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;

                context.Response.ContentType = "application/problem+json";

                var problem = exception switch
                {
                    BaseBusinessRuleException businessRuleException =>
                        new ProblemDetails
                        {
                            Title  = "Business rule broken",
                            Status = StatusCodes.Status409Conflict,
                            Detail = businessRuleException.Message
                        },

                    InvalidCommandException invalidCommandException =>
                        new ProblemDetails
                        {
                            Title  = "Command validation error",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = invalidCommandException.Message
                        },

                    _ => new ProblemDetails
                    {
                        Title  = "Internal Server Error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = exception?.Message
                    }
                };

                context.Response.StatusCode =
                    problem.Status ?? StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(problem).ConfigureAwait(false);
            });
        });

        return app;
    }
}
```

**Note:** `BaseBusinessRuleException` is in `HC.Core.Domain`. `InvalidCommandException` is in
`HC.Core.Application`. Both are thrown from the module's application layer and bubble up through
the module facade — the API layer catches them here without knowing anything about the domain.
