using HC.LIS.API.Modules.Analyzer.AnalyzerSamples.DispatchSampleInfo;
using HC.LIS.API.Modules.Analyzer.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.API.Modules.Analyzer.AnalyzerSamples.GetAnalyzerSampleExamDetails;
using HC.LIS.API.Modules.Analyzer.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.API.Modules.Analyzer.AnalyzerSamples.ReceiveExamResult;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;

namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples;

internal static class AnalyzerSamplesEndpoints
{
    internal static RouteGroupBuilder MapAnalyzerSamplesEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("AnalyzerSamples");

        group.MapGet("{id:guid}", GetAnalyzerSampleDetailsEndpoint.Handle)
            .WithName("GetAnalyzerSampleDetails")
            .WithSummary("Get analyzer sample details by ID.")
            .Produces<AnalyzerSampleDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/exams", GetAnalyzerSampleExamDetailsEndpoint.Handle)
            .WithName("GetAnalyzerSampleExamDetails")
            .WithSummary("Get exam details for an analyzer sample.")
            .Produces<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>>();

        group.MapGet("by-barcode/{barcode}", GetSampleInfoByBarcodeEndpoint.Handle)
            .WithName("GetSampleInfoByBarcode")
            .WithSummary("Get sample info by barcode.")
            .Produces<SampleInfoDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{id:guid}/dispatch", DispatchSampleInfoEndpoint.Handle)
            .WithName("DispatchSampleInfo")
            .WithSummary("Dispatch sample information to the analyzer.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/exam-results", ReceiveExamResultEndpoint.Handle)
            .WithName("ReceiveExamResult")
            .WithSummary("Receive an exam result from the analyzer.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
