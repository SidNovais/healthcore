using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;

namespace HC.LIS.API.Modules.TestOrders.Physicians.DeactivatePhysician;

internal static class DeactivatePhysicianEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new DeactivatePhysicianCommand(id, SystemClock.Now)).ConfigureAwait(false);
        return TypedResults.NoContent();
    }
}
