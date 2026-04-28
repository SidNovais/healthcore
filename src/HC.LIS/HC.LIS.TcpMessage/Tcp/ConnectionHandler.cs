using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.BuildMessageAck;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.TcpMessage.Configuration;
using HC.LIS.TcpMessage.Mllp;
using Microsoft.Extensions.Logging;

namespace HC.LIS.TcpMessage.Tcp;

internal sealed partial class ConnectionHandler(
    IAnalyzerModule analyzerModule,
    TcpOptions options,
    ILogger<ConnectionHandler> logger)
{
    internal async Task HandleAsync(Stream stream, string remoteIp, SemaphoreSlim semaphore, CancellationToken ct)
    {
        var state = ConnectionState.ReceivingQuery;
        try
        {
            byte[] rawQuery = await MllpFramer.UnwrapAsync(stream, options.EnableMllpChecksum, ct).ConfigureAwait(false);
            state = ConnectionState.QueryAnswered;

            byte[] rspBytes = await analyzerModule
                .ExecuteCommandAsync<byte[]>(new HandleBarcodeQueryCommand(rawQuery))
                .ConfigureAwait(false);

            await stream.WriteAsync(MllpFramer.Wrap(rspBytes, options.EnableMllpChecksum), ct).ConfigureAwait(false);

            state = ConnectionState.ReceivingResult;

            byte[] rawResult = await MllpFramer.UnwrapAsync(stream, options.EnableMllpChecksum, ct).ConfigureAwait(false);

            byte[] ackBytes = await analyzerModule
                .ExecuteCommandAsync<byte[]>(new BuildMessageAckCommand(rawResult))
                .ConfigureAwait(false);

            await stream.WriteAsync(MllpFramer.Wrap(ackBytes, options.EnableMllpChecksum), ct).ConfigureAwait(false);

            await analyzerModule
                .ExecuteCommandAsync(new ForwardRawResultCommand(rawResult))
                .ConfigureAwait(false);

            state = ConnectionState.Done;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.ExchangeError(logger, ex, remoteIp, state);
            stream.Close();
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Error,
            Message = "TCP exchange error from {RemoteIp} in state {State}")]
        internal static partial void ExchangeError(
            ILogger logger, Exception ex, string remoteIp, ConnectionState state);
    }
}
