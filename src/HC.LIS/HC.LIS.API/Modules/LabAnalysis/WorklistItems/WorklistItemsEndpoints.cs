namespace HC.LIS.API.Modules.LabAnalysis.WorklistItems;

internal static class WorklistItemsEndpoints
{
    internal static RouteGroupBuilder MapWorklistItemsEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("WorklistItems");

        // TODO: add endpoint registrations using /create-api add
        // Example:
        // group.MapGet("{id:guid}", GetWorklistItemEndpoint.Handle)
        //     .WithName("GetWorklistItem")
        //     .WithSummary("Get a worklist item by ID.")
        //     .Produces<WorklistItemDto>()
        //     .ProducesProblem(401)
        //     .ProducesProblem(404);

        return group;
    }
}
