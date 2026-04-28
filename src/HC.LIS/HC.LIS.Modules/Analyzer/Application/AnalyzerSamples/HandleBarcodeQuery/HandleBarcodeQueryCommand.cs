using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;

public sealed class HandleBarcodeQueryCommand : CommandBase<byte[]>
{
    public ReadOnlyMemory<byte> RawQueryPayload { get; }

    public HandleBarcodeQueryCommand(byte[] rawQueryPayload)
    {
        RawQueryPayload = rawQueryPayload;
    }
}
