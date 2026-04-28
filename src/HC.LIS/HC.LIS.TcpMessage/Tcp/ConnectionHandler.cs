using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HC.LIS.TcpMessage.Tcp;

internal sealed class ConnectionHandler
{
#pragma warning disable CA1822
    internal Task HandleAsync(Stream stream, string remoteIp, CancellationToken ct)
        => throw new NotImplementedException("Implemented in Phase 5");
#pragma warning restore CA1822
}
