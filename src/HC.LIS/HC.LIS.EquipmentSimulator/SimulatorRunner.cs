using System.Net.Sockets;
using System.Text;
using HC.LIS.EquipmentSimulator.Hl7;
using HC.LIS.EquipmentSimulator.Mllp;

namespace HC.LIS.EquipmentSimulator;

internal sealed class SimulatorRunner(SimulatorOptions options)
{
    internal async Task<int> RunAsync()
    {
        Log.Information("Connecting to {Host}:{Port}...", options.Host, options.Port);
        using var client = new TcpClient();
        await client.ConnectAsync(options.Host, options.Port).ConfigureAwait(false);
        var stream = client.GetStream();
        var ct = CancellationToken.None;

        var query = QueryMessageBuilder.Build(options.Barcode);
        var queryFrame = MllpFramer.Wrap(Encoding.UTF8.GetBytes(query), includeChecksum: false);
        await stream.WriteAsync(queryFrame, ct).ConfigureAwait(false);
        Log.Information("Sent QBP^Q11 for barcode {Barcode}", options.Barcode);

        var responseBytes = await MllpFramer.UnwrapAsync(stream, validateChecksum: false, ct).ConfigureAwait(false);
        var response = Encoding.UTF8.GetString(responseBytes);
        var mnemonics = ResponseParser.ParseExamMnemonics(response);
        Log.Information("Received RSP^K11 — exams: {Exams}", string.Join(", ", mnemonics));
        Log.Information("Waiting {DelayMs}ms (simulating analysis)...", options.DelayMs);
        await Task.Delay(options.DelayMs, ct).ConfigureAwait(false);

        var results = Array.ConvertAll(mnemonics, ResultValueGenerator.Generate);
        var resultMessage = ResultMessageBuilder.Build(options.Barcode, results);
        var resultFrame = MllpFramer.Wrap(Encoding.UTF8.GetBytes(resultMessage), includeChecksum: false);
        await stream.WriteAsync(resultFrame, ct).ConfigureAwait(false);
        foreach (var result in results)
            Log.Information("  {Mnemonic} = {Value} {Unit} (ref: {ReferenceRange})", result.Mnemonic, result.Value, result.Unit, result.ReferenceRange);

        var ackBytes = await MllpFramer.UnwrapAsync(stream, validateChecksum: false, ct).ConfigureAwait(false);
        var ack = Encoding.UTF8.GetString(ackBytes);
        if (!AckParser.IsAA(ack))
        {
            Log.Error("ACK was not AA — integration FAILED");
            return 1;
        }

        Log.Information("Received ACK AA — integration OK");
        return 0;
    }
}
