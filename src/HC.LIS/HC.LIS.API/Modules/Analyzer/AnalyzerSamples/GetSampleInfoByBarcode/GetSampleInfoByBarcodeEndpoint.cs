using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples.GetSampleInfoByBarcode;

internal static class GetSampleInfoByBarcodeEndpoint
{
    internal static async Task<IResult> Handle(
        string barcode,
        IAnalyzerModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetSampleInfoByBarcodeQuery(barcode)).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
