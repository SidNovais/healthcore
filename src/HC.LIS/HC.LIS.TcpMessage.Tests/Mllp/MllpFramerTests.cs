using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.TcpMessage.Mllp;

namespace HC.LIS.TcpMessage.Tests.Mllp;

public class MllpFramerTests
{
    [Fact]
    public void WrapPrependsSobAndAppendsEob()
    {
        byte[] payload = [0x41, 0x42, 0x43];

        byte[] result = MllpFramer.Wrap(payload, includeChecksum: false);

        result[0].Should().Be(0x0B);
        result[^2].Should().Be(0x1C);
        result[^1].Should().Be(0x0D);
        result[1..^2].Should().Equal(payload);
    }

    [Fact]
    public void WrapAppendsBccChecksumByteWhenEnabled()
    {
        byte[] payload = [0x41, 0x42, 0x43];
        byte expectedChecksum = (byte)((0x41 + 0x42 + 0x43) % 256);

        byte[] result = MllpFramer.Wrap(payload, includeChecksum: true);

        result[^3].Should().Be(expectedChecksum);
    }

    [Fact]
    public async Task UnwrapAsyncReturnsInnerPayload()
    {
        byte[] payload = [0x41, 0x42, 0x43];
        byte[] frame = [0x0B, 0x41, 0x42, 0x43, 0x1C, 0x0D];
        using var stream = new MemoryStream(frame);

        byte[] result = await MllpFramer.UnwrapAsync(stream, validateChecksum: false, CancellationToken.None);

        result.Should().Equal(payload);
    }

    [Fact]
    public async Task UnwrapAsyncStripsBccChecksumByteWhenEnabled()
    {
        byte[] payload = [0x41, 0x42, 0x43];
        byte checksum = MllpFramer.ComputeChecksum(payload);
        byte[] frame = [0x0B, 0x41, 0x42, 0x43, checksum, 0x1C, 0x0D];
        using var stream = new MemoryStream(frame);

        byte[] result = await MllpFramer.UnwrapAsync(stream, validateChecksum: true, CancellationToken.None);

        result.Should().Equal(payload);
    }

    [Fact]
    public async Task UnwrapAsyncThrowsOnChecksumMismatch()
    {
        byte[] frame = [0x0B, 0x41, 0x42, 0x43, 0xFF, 0x1C, 0x0D];
        using var stream = new MemoryStream(frame);

        var exception = await FluentActions
            .Awaiting(() => MllpFramer.UnwrapAsync(stream, validateChecksum: true, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Be("MLLP checksum mismatch");
    }

    [Fact]
    public async Task UnwrapAsyncThrowsOnTruncatedFrame()
    {
        byte[] frame = [0x0B, 0x41, 0x42, 0x43];
        using var stream = new MemoryStream(frame);

        var exception = await FluentActions
            .Awaiting(() => MllpFramer.UnwrapAsync(stream, validateChecksum: false, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Be("MLLP frame truncated");
    }
}
