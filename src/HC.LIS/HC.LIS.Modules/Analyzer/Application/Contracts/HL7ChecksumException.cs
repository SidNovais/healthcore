namespace HC.LIS.Modules.Analyzer.Application.Contracts;

public class HL7ChecksumException : Exception
{
    public byte Expected { get; }
    public byte Actual { get; }

    public HL7ChecksumException() { }
    public HL7ChecksumException(string message) : base(message) { }
    public HL7ChecksumException(string message, Exception innerException) : base(message, innerException) { }

    public HL7ChecksumException(byte expected, byte actual)
        : base($"HL7 content checksum mismatch: expected {expected}, got {actual}")
    {
        Expected = expected;
        Actual = actual;
    }
}
