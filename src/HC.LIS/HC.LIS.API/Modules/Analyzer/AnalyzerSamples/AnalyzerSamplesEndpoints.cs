namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples;

internal static class AnalyzerSamplesEndpoints
{
    internal static RouteGroupBuilder MapAnalyzerSamplesEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("AnalyzerSamples");

        // TODO: add endpoint registrations using /create-api add
        // Example:
        // group.MapGet("{id:guid}", GetAnalyzerSampleEndpoint.Handle)
        //     .WithName("GetAnalyzerSample")
        //     .WithSummary("Get an analyzer sample by ID.")
        //     .Produces<AnalyzerSampleDto>()
        //     .ProducesProblem(401)
        //     .ProducesProblem(404);

        return group;
    }
}
