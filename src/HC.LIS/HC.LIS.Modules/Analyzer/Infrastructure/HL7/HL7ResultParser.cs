using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Infrastructure.HL7;

internal class HL7ResultParser : IHL7ResultParser
{
    public AnalyzerResultDto Parse(string hl7Message)
    {
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
}
