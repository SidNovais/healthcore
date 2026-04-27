using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HC.LIS.TcpMessage.Mllp;

internal static class MllpFramer
{
    private const byte Sob = 0x0B;
    private const byte EbHigh = 0x1C;
    private const byte EbLow = 0x0D;

    internal static byte[] Wrap(byte[] payload, bool includeChecksum)
    {
        var frame = new List<byte>(payload.Length + 4) { Sob };
        frame.AddRange(payload);
        if (includeChecksum)
            frame.Add(ComputeChecksum(payload));
        frame.Add(EbHigh);
        frame.Add(EbLow);
        return frame.ToArray();
    }

    internal static async Task<byte[]> UnwrapAsync(Stream stream, bool validateChecksum, CancellationToken ct)
    {
        var buf = new byte[1];

        if (await stream.ReadAsync(buf, ct).ConfigureAwait(false) == 0)
            throw new InvalidOperationException("MLLP frame truncated");

        var body = new List<byte>();

        while (true)
        {
            if (await stream.ReadAsync(buf, ct).ConfigureAwait(false) == 0)
                throw new InvalidOperationException("MLLP frame truncated");

            if (buf[0] == EbHigh)
            {
                if (await stream.ReadAsync(buf, ct).ConfigureAwait(false) == 0)
                    throw new InvalidOperationException("MLLP frame truncated");
                break;
            }

            body.Add(buf[0]);
        }

        if (!validateChecksum)
            return body.ToArray();

        byte received = body[^1];
        byte[] payload = body.Take(body.Count - 1).ToArray();

        if (received != ComputeChecksum(payload))
            throw new InvalidOperationException("MLLP checksum mismatch");

        return payload;
    }

    internal static byte ComputeChecksum(byte[] payload) =>
        payload.Aggregate((byte)0, (acc, b) => (byte)((acc + b) % 256));
}
