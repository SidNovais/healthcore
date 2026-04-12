using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples.ReceiveExamResult;

internal static class ReceiveExamResultEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        ReceiveExamResultRequest request,
        IAnalyzerModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new ReceiveExamResultCommand(
            id,
            request.ExamMnemonic,
            request.ResultValue,
            request.ResultUnit,
            request.ReferenceRange,
            request.InstrumentId,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
