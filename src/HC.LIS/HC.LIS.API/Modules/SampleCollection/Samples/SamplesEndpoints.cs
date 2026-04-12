using HC.LIS.API.Modules.SampleCollection.Samples.GetSampleDetails;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;

namespace HC.LIS.API.Modules.SampleCollection.Samples;

internal static class SamplesEndpoints
{
    internal static RouteGroupBuilder MapSamplesEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("Samples");

        group.MapGet("{id:guid}", GetSampleDetailsEndpoint.Handle)
            .WithName("GetSampleDetails")
            .WithSummary("Get sample details by ID.")
            .Produces<SampleDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
