using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

namespace HC.LIS.API.Modules.TestOrders.Physicians.UpdatePhysician;

internal static class UpdatePhysicianEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        UpdatePhysicianRequest request,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new UpdatePhysicianCommand(
            id,
            request.FullName,
            request.LicenceNumber,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
