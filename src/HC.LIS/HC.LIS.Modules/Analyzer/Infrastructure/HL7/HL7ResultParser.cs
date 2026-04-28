using System.Text;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Infrastructure.HL7;

internal class HL7ResultParser(bool enableHl7Checksum) : IHL7ResultParser
{
    private readonly bool _enableHl7Checksum = enableHl7Checksum;

    public AnalyzerResultDto Parse(string hl7Message)
    {
        if (_enableHl7Checksum)
            ValidateChecksum(hl7Message);
        // Simulated HL7 v2.x ORU^R01 parser — expects pipe-delimited lines
        // OBX: OBX|1|NM|<ExamMnemonic>^<Desc>||<Value>|<Unit>|<RefRange>||...
        // SPM: SPM|||<Barcode>
        var lines = hl7Message.Split('\r', StringSplitOptions.RemoveEmptyEntries);

        string barcode = ExtractField(lines, "SPM", 3);
        string examMnemonic = ExtractField(lines, "OBX", 3).Split('^')[0];
        string resultValue = ExtractField(lines, "OBX", 5);
        string resultUnit = ExtractField(lines, "OBX", 6);
        string referenceRange = ExtractField(lines, "OBX", 7);

        return new AnalyzerResultDto(
            SampleBarcode: barcode,
            ExamMnemonic: examMnemonic,
            ResultValue: resultValue,
            ResultUnit: resultUnit,
            ReferenceRange: referenceRange,
            InstrumentId: Guid.Empty,   // real impl: parse MSH sending facility ID
            RecordedAt: DateTime.UtcNow // real impl: parse OBX observation datetime
        );
    }

    private static string ExtractField(string[] lines, string segment, int fieldIndex)
    {
        string? line = Array.Find(lines, l => l.StartsWith(segment + "|", StringComparison.Ordinal));
        if (line is null) return string.Empty;
        string[] fields = line.Split('|');
        return fieldIndex < fields.Length ? fields[fieldIndex] : string.Empty;
    }

    private static void ValidateChecksum(string hl7Message)
    {
        int zcsIndex = hl7Message.LastIndexOf("\rZCS|", StringComparison.Ordinal);
        if (zcsIndex < 0)
            throw new HL7ChecksumException(0, 0);

        string zcsLine = hl7Message[(zcsIndex + 1)..].TrimEnd('\r');
        string[] zcsFields = zcsLine.Split('|');
        if (zcsFields.Length < 2 || !byte.TryParse(zcsFields[1], out byte expected))
            throw new HL7ChecksumException(0, 0);

        byte[] payloadBytes = Encoding.UTF8.GetBytes(hl7Message[..(zcsIndex + 1)]);
        byte actual = ComputeBcc(payloadBytes);
        if (actual != expected)
            throw new HL7ChecksumException(expected, actual);
    }

    private static byte ComputeBcc(byte[] bytes)
    {
        int sum = 0;
        foreach (byte b in bytes)
            sum += b;
        return (byte)(sum % 256);
    }
}
