namespace HC.LIS.API.Modules.SampleCollection.Samples;

internal static class SamplesEndpoints
{
    internal static RouteGroupBuilder MapSamplesEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("Samples");

        // TODO: add endpoint registrations using /create-api add
        // Example:
        // group.MapGet("{id:guid}", GetSampleEndpoint.Handle)
        //     .WithName("GetSample")
        //     .WithSummary("Get a sample by ID.")
        //     .Produces<SampleDto>()
        //     .ProducesProblem(401)
        //     .ProducesProblem(404);

        return group;
    }
}
