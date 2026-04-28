using System.Text;
using HC.Core.Application;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.BuildMessageAck;

internal class BuildMessageAckCommandHandler : ICommandHandler<BuildMessageAckCommand, byte[]>
{
    public Task<byte[]> Handle(BuildMessageAckCommand command, CancellationToken cancellationToken)
    {
        string rawHl7 = Encoding.UTF8.GetString(command.RawPayload.Span);
        string[] lines = rawHl7.Split('\r', StringSplitOptions.RemoveEmptyEntries);

        string mshLine = Array.Find(lines, l => l.StartsWith("MSH|", StringComparison.Ordinal))
            ?? throw new InvalidCommandException("Missing MSH segment in HL7 payload");

        string[] fields = mshLine.Split('|');
        string msgControlId = fields.Length > 9  ? fields[9]  : string.Empty;
        string version      = fields.Length > 11 ? fields[11] : "2.5";

        string ack =
            $"MSH|^~\\&|HC.LIS||ANALYZER||{SystemClock.Now:yyyyMMddHHmmss}||ACK^R01|{Guid.NewGuid()}|P|{version}\r" +
            $"MSA|AA|{msgControlId}";

        return Task.FromResult(Encoding.UTF8.GetBytes(ack));
    }
}
