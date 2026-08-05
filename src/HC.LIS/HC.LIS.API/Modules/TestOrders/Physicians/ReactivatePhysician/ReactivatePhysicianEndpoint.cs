using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;

namespace HC.LIS.API.Modules.TestOrders.Physicians.ReactivatePhysician;

internal static class ReactivatePhysicianEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new ReactivatePhysicianCommand(id, SystemClock.Now)).ConfigureAwait(false);
        return TypedResults.NoContent();
    }
}
