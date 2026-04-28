using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.BuildMessageAck;

public sealed class BuildMessageAckCommand : CommandBase<byte[]>
{
    public ReadOnlyMemory<byte> RawPayload { get; }

    public BuildMessageAckCommand(byte[] rawPayload)
    {
        RawPayload = rawPayload;
    }
}
