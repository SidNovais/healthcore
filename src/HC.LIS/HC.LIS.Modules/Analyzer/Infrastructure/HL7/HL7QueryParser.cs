using System.Text;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;

namespace HC.LIS.Modules.Analyzer.Infrastructure.HL7;

internal class HL7QueryParser(bool enableHl7Checksum) : IHL7QueryParser
{
    private readonly bool _enableHl7Checksum = enableHl7Checksum;

    public string ParseBarcode(byte[] rawQueryPayload)
    {
        // Simulated HL7 v2.x QBP^Q11 parser — expects pipe-delimited CRLF/CR lines
        // QPD: QPD|Q11^Sample Info Query^HL70471|<queryId>|<barcode>
        if (_enableHl7Checksum)
            ValidateChecksum(rawQueryPayload);

        string message = Encoding.UTF8.GetString(rawQueryPayload);
        string[] lines = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);

        string? qpdLine = Array.Find(lines, l => l.StartsWith("QPD|", StringComparison.Ordinal));
        if (qpdLine is null)
            return string.Empty;

        string[] fields = qpdLine.Split('|');
        return fields.Length > 3 ? fields[3] : string.Empty;
    }

    private static void ValidateChecksum(byte[] rawQueryPayload)
    {
        // ZCS segment convention: ZCS|<bcc_decimal>\r appended after all other segments.
        // BCC = sum of all preceding bytes mod 256.
        string message = Encoding.UTF8.GetString(rawQueryPayload);
        int zcsIndex = message.LastIndexOf("\rZCS|", StringComparison.Ordinal);
        if (zcsIndex < 0)
            throw new HL7ChecksumException(0, 0);

        string zcsLine = message[(zcsIndex + 1)..].TrimEnd('\r');
        string[] zcsFields = zcsLine.Split('|');
        if (zcsFields.Length < 2 || !byte.TryParse(zcsFields[1], out byte expected))
            throw new HL7ChecksumException(0, 0);

        byte[] payloadBytes = Encoding.UTF8.GetBytes(message[..(zcsIndex + 1)]);
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
