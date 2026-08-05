using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

namespace HC.LIS.API.Modules.TestOrders.Physicians.GetPhysicianDetails;

internal static class GetPhysicianDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        var physician = await module.ExecuteQueryAsync(
            new GetPhysicianDetailsQuery(id)).ConfigureAwait(false);
        return physician is null ? TypedResults.NotFound() : TypedResults.Ok(physician);
    }
}
