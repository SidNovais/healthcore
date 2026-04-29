using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.TcpMessage.Mllp;

namespace HC.LIS.TcpMessage.IntegrationTests.Helpers;

internal sealed class TcpTestClient : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    internal TcpTestClient(int port)
    {
        _client = new TcpClient();
        _client.Connect(IPAddress.Loopback, port);
        _stream = _client.GetStream();
    }

    internal async Task SendAsync(byte[] payload, bool includeChecksum = false, CancellationToken ct = default)
    {
        byte[] frame = MllpFramer.Wrap(payload, includeChecksum);
        await _stream.WriteAsync(frame, ct).ConfigureAwait(false);
    }

    internal async Task<byte[]> ReceiveAsync(bool validateChecksum = false, CancellationToken ct = default)
    {
        return await MllpFramer.UnwrapAsync(_stream, validateChecksum, ct).ConfigureAwait(false);
    }

    internal async Task SendRawAsync(byte[] rawFrame, CancellationToken ct = default)
    {
        await _stream.WriteAsync(rawFrame, ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _stream.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
