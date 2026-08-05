using HC.Core.Domain;
using HC.LIS.API.Common;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

namespace HC.LIS.API.Modules.TestOrders.Physicians.RegisterPhysician;

internal static class RegisterPhysicianEndpoint
{
    internal static async Task<IResult> Handle(
        RegisterPhysicianRequest request,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        var id = await module.ExecuteCommandAsync(new RegisterPhysicianCommand(
            Guid.CreateVersion7(),
            request.FullName,
            request.LicenceNumber,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.Created($"/api/v1/physicians/{id}", new CreatedIdResponse(id));
    }
}
