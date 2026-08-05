using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;

namespace HC.LIS.API.Modules.TestOrders.Physicians.SearchPhysicians;

internal static class SearchPhysiciansEndpoint
{
    internal static async Task<IResult> Handle(
        string? search,
        bool? includeInactive,
        ITestOrdersModule module,
        CancellationToken ct)
    {
        var results = await module.ExecuteQueryAsync(
            new SearchPhysiciansQuery(search, includeInactive ?? false)).ConfigureAwait(false);
        return TypedResults.Ok(results);
    }
}
