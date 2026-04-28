using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.BuildMessageAck;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.TcpMessage.AuditLog;
using HC.LIS.TcpMessage.Configuration;
using HC.LIS.TcpMessage.Mllp;
using HC.LIS.TcpMessage.Tcp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HC.LIS.TcpMessage.Tests.Tcp;

public class ConnectionHandlerTests
{
    private static readonly byte[] QueryPayload = "QBP^Q11"u8.ToArray();
    private static readonly byte[] ResultPayload = "ORU^R01"u8.ToArray();
    private static readonly byte[] RspBytes = "RSP_RESPONSE"u8.ToArray();
    private static readonly byte[] AckBytes = "ACK_RESPONSE"u8.ToArray();

    private static ConnectionHandler CreateHandler(IAnalyzerModule module)
        => new(
            module,
            new TcpAuditLogger(NullLogger<TcpAuditLogger>.Instance),
            new TcpOptions(),
            NullLogger<ConnectionHandler>.Instance);

    private static byte[] BuildFullExchangeData()
    {
        byte[] query = MllpFramer.Wrap(QueryPayload, includeChecksum: false);
        byte[] result = MllpFramer.Wrap(ResultPayload, includeChecksum: false);
        var combined = new byte[query.Length + result.Length];
        query.CopyTo(combined, 0);
        result.CopyTo(combined, query.Length);
        return combined;
    }

    [Fact]
    public async Task FullExchangeQueryPhaseCallsHandleBarcodeQueryCommand()
    {
        var module = Substitute.For<IAnalyzerModule>();
        module.ExecuteCommandAsync<byte[]>(Arg.Any<HandleBarcodeQueryCommand>())
            .Returns(Task.FromResult(RspBytes));
        module.ExecuteCommandAsync<byte[]>(Arg.Any<BuildMessageAckCommand>())
            .Returns(Task.FromResult(AckBytes));
        module.ExecuteCommandAsync(Arg.Any<ForwardRawResultCommand>())
            .Returns(Task.CompletedTask);

        using var stream = new DuplexStream(BuildFullExchangeData());
        using var semaphore = new SemaphoreSlim(0, 1);

        await CreateHandler(module).HandleAsync(stream, "127.0.0.1", semaphore, CancellationToken.None);

        await module.Received(1).ExecuteCommandAsync<byte[]>(Arg.Any<HandleBarcodeQueryCommand>());
        stream.WrittenData.Should().StartWith(MllpFramer.Wrap(RspBytes, includeChecksum: false));
    }

    [Fact]
    public async Task FullExchangeResultPhaseSendsImmediateAckBeforeDomainProcessing()
    {
        var events = new List<string>();
        var module = Substitute.For<IAnalyzerModule>();
        module.ExecuteCommandAsync<byte[]>(Arg.Any<HandleBarcodeQueryCommand>())
            .Returns(Task.FromResult(RspBytes));
        module.ExecuteCommandAsync<byte[]>(Arg.Any<BuildMessageAckCommand>())
            .Returns(Task.FromResult(AckBytes));
        module.ExecuteCommandAsync(Arg.Any<ForwardRawResultCommand>())
            .Returns(_ => { events.Add("forward"); return Task.CompletedTask; });

        using var stream = new DuplexStream(BuildFullExchangeData(), events, "write");
        using var semaphore = new SemaphoreSlim(0, 1);

        await CreateHandler(module).HandleAsync(stream, "127.0.0.1", semaphore, CancellationToken.None);

        int lastWriteIndex = events.LastIndexOf("write");
        int forwardIndex = events.IndexOf("forward");
        lastWriteIndex.Should().BeLessThan(forwardIndex,
            "the ACK must be written before ForwardRawResultCommand is dispatched");
    }

    [Fact]
    public async Task FullExchangeResultPhaseCallsForwardRawResultCommandAfterAck()
    {
        var module = Substitute.For<IAnalyzerModule>();
        module.ExecuteCommandAsync<byte[]>(Arg.Any<HandleBarcodeQueryCommand>())
            .Returns(Task.FromResult(RspBytes));
        module.ExecuteCommandAsync<byte[]>(Arg.Any<BuildMessageAckCommand>())
            .Returns(Task.FromResult(AckBytes));
        module.ExecuteCommandAsync(Arg.Any<ForwardRawResultCommand>())
            .Returns(Task.CompletedTask);

        using var stream = new DuplexStream(BuildFullExchangeData());
        using var semaphore = new SemaphoreSlim(0, 1);

        await CreateHandler(module).HandleAsync(stream, "127.0.0.1", semaphore, CancellationToken.None);

        await module.Received(1).ExecuteCommandAsync(Arg.Any<ForwardRawResultCommand>());
    }

    [Fact]
    public async Task ReadTimeoutSemaphoreIsReleased()
    {
        var module = Substitute.For<IAnalyzerModule>();
        using var stream = new DuplexStream([0x0B]); // truncated — no EOB, causes MLLP frame truncated error
        using var semaphore = new SemaphoreSlim(0, 1);

        await CreateHandler(module).HandleAsync(stream, "127.0.0.1", semaphore, CancellationToken.None);

        semaphore.CurrentCount.Should().Be(1, "semaphore must be released even when the stream errors");
    }

    private sealed class DuplexStream : Stream
    {
        private readonly MemoryStream _reader;
        private readonly MemoryStream _writer = new();
        private readonly List<string>? _events;
        private readonly string _writeEventName;

        public DuplexStream(byte[] readData, List<string>? events = null, string writeEventName = "write")
        {
            _reader = new MemoryStream(readData);
            _events = events;
            _writeEventName = writeEventName;
        }

        public byte[] WrittenData => _writer.ToArray();

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            _reader.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) =>
            _reader.ReadAsync(buffer, offset, count, ct);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) =>
            _reader.ReadAsync(buffer, ct);

        public override void Write(byte[] buffer, int offset, int count)
        {
            _events?.Add(_writeEventName);
            _writer.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            _events?.Add(_writeEventName);
            return _writer.WriteAsync(buffer, offset, count, ct);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            _events?.Add(_writeEventName);
            return _writer.WriteAsync(buffer, ct);
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
                _writer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
