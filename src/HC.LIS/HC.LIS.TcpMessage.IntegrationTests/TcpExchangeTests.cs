using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.TcpMessage.IntegrationTests.Helpers;
using HC.LIS.TcpMessage.Mllp;

namespace HC.LIS.TcpMessage.IntegrationTests;

// ─── Probes ──────────────────────────────────────────────────────────────────

file sealed class AnalyzerSampleDetailsProbe(
    Guid analyzerSampleId,
    IAnalyzerModule module,
    Func<AnalyzerSampleDetailsDto?, bool>? satisfied = null
) : IProbe<AnalyzerSampleDetailsDto>
{
    private readonly Func<AnalyzerSampleDetailsDto?, bool> _satisfied =
        satisfied ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"AnalyzerSampleDetails not found or condition unmet for {analyzerSampleId}";

    public async Task<AnalyzerSampleDetailsDto?> GetSampleAsync() =>
        await module.ExecuteQueryAsync(new GetAnalyzerSampleDetailsQuery(analyzerSampleId))
            .ConfigureAwait(false);

    public bool IsSatisfied(AnalyzerSampleDetailsDto? sample) => _satisfied(sample);
}

file sealed class AnalyzerSampleExamDetailsProbe(
    Guid analyzerSampleId,
    IAnalyzerModule module,
    Func<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>?, bool>? satisfied = null
) : IProbe<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>>
{
    private readonly Func<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>?, bool> _satisfied =
        satisfied ?? (dtos => dtos is { Count: > 0 });

    public string DescribeFailureTo() =>
        $"AnalyzerSampleExamDetails not found or condition unmet for {analyzerSampleId}";

    public async Task<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>?> GetSampleAsync() =>
        await module.ExecuteQueryAsync(new GetAnalyzerSampleExamDetailsQuery(analyzerSampleId))
            .ConfigureAwait(false);

    public bool IsSatisfied(IReadOnlyCollection<AnalyzerSampleExamDetailsDto>? sample) =>
        _satisfied(sample);
}

// ─── HL7 message builder ─────────────────────────────────────────────────────

file static class HL7MessageBuilder
{
    internal static byte[] BuildQbpQ11(string barcode, string msgControlId) =>
        Encoding.UTF8.GetBytes(
            $"MSH|^~\\&|HC.LIS||ANALYZER||20260429120000||QBP^Q11|{msgControlId}|P|2.5\r" +
            $"QPD|Q11^Sample Info Query^HL70471|QRY001|{barcode}\r" +
            $"RCP|I");

    internal static byte[] BuildQbpQ11WithHl7Checksum(string barcode, string msgControlId)
    {
        string baseHl7 =
            $"MSH|^~\\&|HC.LIS||ANALYZER||20260429120000||QBP^Q11|{msgControlId}|P|2.5\r" +
            $"QPD|Q11^Sample Info Query^HL70471|QRY001|{barcode}\r" +
            $"RCP|I\r";
        byte[] baseBytes = Encoding.UTF8.GetBytes(baseHl7);
        byte bcc = ComputeBcc(baseBytes);
        return Encoding.UTF8.GetBytes(baseHl7 + $"ZCS|{bcc}");
    }

    internal static byte[] BuildQbpQ11WithWrongHl7Checksum(string barcode, string msgControlId)
    {
        string baseHl7 =
            $"MSH|^~\\&|HC.LIS||ANALYZER||20260429120000||QBP^Q11|{msgControlId}|P|2.5\r" +
            $"QPD|Q11^Sample Info Query^HL70471|QRY001|{barcode}\r" +
            $"RCP|I\r";
        byte[] baseBytes = Encoding.UTF8.GetBytes(baseHl7);
        // Flip all bits of the correct BCC → guaranteed mismatch
        byte bcc = (byte)(ComputeBcc(baseBytes) ^ 0xFF);
        return Encoding.UTF8.GetBytes(baseHl7 + $"ZCS|{bcc}");
    }

    internal static byte[] BuildOruR01(string barcode, string examMnemonic, string resultValue, string msgControlId) =>
        Encoding.UTF8.GetBytes(
            $"MSH|^~\\&|ANALYZER||HC.LIS||20260429120000||ORU^R01|{msgControlId}|P|2.5\r" +
            $"OBX|1|NM|{examMnemonic}^{examMnemonic} Description||{resultValue}|mmol/L|3.5-5.5\r" +
            $"SPM|||{barcode}");

    internal static byte[] BuildCorruptedMllpFrame(string barcode, string msgControlId)
    {
        byte[] payload = BuildQbpQ11(barcode, msgControlId);
        byte[] frame = MllpFramer.Wrap(payload, includeChecksum: true);
        // BCC byte sits at frame[^3]; flip all bits → guaranteed checksum mismatch
        frame[^3] ^= 0xFF;
        return frame;
    }

    private static byte ComputeBcc(byte[] bytes)
    {
        int sum = 0;
        foreach (byte b in bytes) sum += b;
        return (byte)(sum % 256);
    }
}

// ─── Tests: standard (no checksums) ─────────────────────────────────────────

public class TcpExchangeTests() : TestBase()
{
    [Fact]
    public async Task BarcodeQueryExchangeReturnsRspK11Response()
    {
        var (sampleId, barcode, _) = await SeedAndWaitAsync("TCPQ001", "GLUC");

        using var client = new TcpTestClient(BoundPort);
        await client.SendAsync(HL7MessageBuilder.BuildQbpQ11(barcode, "MSG001"));
        byte[] rspBytes = await client.ReceiveAsync();

        string rsp = Encoding.UTF8.GetString(rspBytes);
        rsp.Should().Contain("RSP^K11");

        var details = await GetEventually(
            new AnalyzerSampleDetailsProbe(sampleId, AnalyzerModule, d => d?.Status == "InfoDispatched"),
            timeoutMs: 15_000);
        details.Should().NotBeNull("sample status should be InfoDispatched after barcode query");
        details!.Status.Should().Be("InfoDispatched");
    }

    [Fact]
    public async Task ResultForwardExchangeSendsAckAndProcessesDomain()
    {
        const string examMnemonic = "GLUC";
        var (sampleId, barcode, _) = await SeedAndWaitAsync("TCPQ002", examMnemonic);

        // Phase 1: barcode query → sets status to InfoDispatched
        using var qbpClient = new TcpTestClient(BoundPort);
        await qbpClient.SendAsync(HL7MessageBuilder.BuildQbpQ11(barcode, "MSG001"));
        await qbpClient.ReceiveAsync();

        var dispatched = await GetEventually(
            new AnalyzerSampleDetailsProbe(sampleId, AnalyzerModule, d => d?.Status == "InfoDispatched"),
            timeoutMs: 15_000);
        dispatched.Should().NotBeNull("sample should reach InfoDispatched before result can be forwarded");

        // Phase 2: result forward → immediate ACK, then domain processing
        using var oruClient = new TcpTestClient(BoundPort);
        await oruClient.SendAsync(HL7MessageBuilder.BuildOruR01(barcode, examMnemonic, "7.5", "MSG002"));
        byte[] ackBytes = await oruClient.ReceiveAsync();

        string ack = Encoding.UTF8.GetString(ackBytes);
        ack.Should().Contain("MSA|AA");
        ack.Should().Contain("MSG002", "ACK MSA-2 must echo the original MSH-10");

        var exams = await GetEventually(
            new AnalyzerSampleExamDetailsProbe(
                sampleId,
                AnalyzerModule,
                dtos => dtos?.Count > 0 && dtos.Any(e => e.ResultValue == "7.5")),
            timeoutMs: 15_000);
        exams.Should().NotBeNull("exam result should be projected within 15 seconds");
        exams!.Should().Contain(e => e.ResultValue == "7.5");
    }

    [Fact]
    public async Task SecondConnectionWaitsForFirstExchangeToComplete()
    {
        var (_, barcode1, _) = await SeedAndWaitAsync("TCPQ003", "GLUC");
        var (_, barcode2, _) = await SeedAndWaitAsync("TCPQ004", "HBA1C");

        using var client1 = new TcpTestClient(BoundPort);
        using var client2 = new TcpTestClient(BoundPort);

        var task1 = DoQbpExchangeAsync(client1, barcode1, "MSG001");
        var task2 = DoQbpExchangeAsync(client2, barcode2, "MSG002");

        string[] responses = await Task.WhenAll(task1, task2);

        responses[0].Should().Contain("RSP^K11");
        responses[1].Should().Contain("RSP^K11");
    }

    private async Task<(Guid SampleId, string Barcode, string ExamMnemonic)> SeedAndWaitAsync(
        string barcode, string examMnemonic)
    {
        var sampleId = Guid.CreateVersion7();
        await AnalyzerModule.ExecuteCommandAsync(new CreateAnalyzerSampleCommand(
            analyzerSampleId: sampleId,
            sampleId: Guid.CreateVersion7(),
            patientId: Guid.CreateVersion7(),
            sampleBarcode: barcode,
            patientName: "Integration Test Patient",
            patientBirthdate: new DateTime(1985, 6, 15),
            patientGender: "M",
            isUrgent: false,
            exams: [new ExamInfoDto(Guid.CreateVersion7(), examMnemonic)],
            createdAt: SystemClock.Now));

        var details = await GetEventually(
            new AnalyzerSampleDetailsProbe(sampleId, AnalyzerModule),
            timeoutMs: 15_000);
        details.Should().NotBeNull($"seeded sample '{barcode}' should appear in projection within 15 seconds");

        return (sampleId, barcode, examMnemonic);
    }

    private static async Task<string> DoQbpExchangeAsync(TcpTestClient client, string barcode, string msgId)
    {
        await client.SendAsync(HL7MessageBuilder.BuildQbpQ11(barcode, msgId));
        byte[] response = await client.ReceiveAsync();
        return Encoding.UTF8.GetString(response);
    }
}

// ─── Tests: MLLP BCC checksum ────────────────────────────────────────────────

public class TcpMllpChecksumTests() : TestBase(enableMllpChecksum: true)
{
    [Fact]
    public async Task CorrectBccByteExchangeSucceeds()
    {
        var sampleId = Guid.CreateVersion7();
        await AnalyzerModule.ExecuteCommandAsync(new CreateAnalyzerSampleCommand(
            analyzerSampleId: sampleId,
            sampleId: Guid.CreateVersion7(),
            patientId: Guid.CreateVersion7(),
            sampleBarcode: "TCPCS001",
            patientName: "Checksum Patient",
            patientBirthdate: new DateTime(1990, 3, 20),
            patientGender: "F",
            isUrgent: false,
            exams: [new ExamInfoDto(Guid.CreateVersion7(), "GLUC")],
            createdAt: SystemClock.Now));

        var details = await GetEventually(
            new AnalyzerSampleDetailsProbe(sampleId, AnalyzerModule),
            timeoutMs: 15_000);
        details.Should().NotBeNull("seeded sample should appear in projection within 15 seconds");

        using var client = new TcpTestClient(BoundPort);
        // includeChecksum=true → MllpFramer.Wrap appends a BCC byte before the EOB markers
        await client.SendAsync(HL7MessageBuilder.BuildQbpQ11("TCPCS001", "MSGCS1"), includeChecksum: true);
        // validateChecksum=true → verify the server also included a correct BCC in its RSP
        byte[] rspBytes = await client.ReceiveAsync(validateChecksum: true);

        string rsp = Encoding.UTF8.GetString(rspBytes);
        rsp.Should().Contain("RSP^K11");
    }

    [Fact]
    public async Task WrongBccByteConnectionClosed()
    {
        using var client = new TcpTestClient(BoundPort);
        // Send a frame whose BCC byte has been deliberately corrupted
        await client.SendRawAsync(HL7MessageBuilder.BuildCorruptedMllpFrame("TCPCS999", "MSGCS2"));

        Func<Task> act = async () => await client.ReceiveAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*frame truncated*");
    }
}

// ─── Tests: HL7 content checksum ─────────────────────────────────────────────

public class TcpHl7ChecksumTests() : TestBase(enableHl7Checksum: true)
{
    [Fact]
    public async Task ValidContentChecksumExchangeSucceeds()
    {
        var sampleId = Guid.CreateVersion7();
        await AnalyzerModule.ExecuteCommandAsync(new CreateAnalyzerSampleCommand(
            analyzerSampleId: sampleId,
            sampleId: Guid.CreateVersion7(),
            patientId: Guid.CreateVersion7(),
            sampleBarcode: "TCPHL001",
            patientName: "HL7 Patient",
            patientBirthdate: new DateTime(1970, 11, 5),
            patientGender: "M",
            isUrgent: false,
            exams: [new ExamInfoDto(Guid.CreateVersion7(), "GLUC")],
            createdAt: SystemClock.Now));

        var details = await GetEventually(
            new AnalyzerSampleDetailsProbe(sampleId, AnalyzerModule),
            timeoutMs: 15_000);
        details.Should().NotBeNull("seeded sample should appear in projection within 15 seconds");

        using var client = new TcpTestClient(BoundPort);
        // Message includes a ZCS segment with the correct BCC of the payload
        await client.SendAsync(HL7MessageBuilder.BuildQbpQ11WithHl7Checksum("TCPHL001", "MSGHL1"));
        byte[] rspBytes = await client.ReceiveAsync();

        string rsp = Encoding.UTF8.GetString(rspBytes);
        rsp.Should().Contain("RSP^K11");
    }

    [Fact]
    public async Task InvalidContentChecksumConnectionClosed()
    {
        using var client = new TcpTestClient(BoundPort);
        // Message includes a ZCS segment with a wrong BCC → HL7QueryParser throws HL7ChecksumException
        await client.SendAsync(HL7MessageBuilder.BuildQbpQ11WithWrongHl7Checksum("TCPHL999", "MSGHL2"));

        Func<Task> act = async () => await client.ReceiveAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*frame truncated*");
    }
}
