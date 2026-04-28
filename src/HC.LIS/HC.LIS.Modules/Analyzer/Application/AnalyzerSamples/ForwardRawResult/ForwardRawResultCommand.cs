using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;

public sealed class ForwardRawResultCommand : CommandBase
{
    public ReadOnlyMemory<byte> RawResultPayload { get; }

    public ForwardRawResultCommand(byte[] rawResultPayload)
    {
        RawResultPayload = rawResultPayload;
    }
}
