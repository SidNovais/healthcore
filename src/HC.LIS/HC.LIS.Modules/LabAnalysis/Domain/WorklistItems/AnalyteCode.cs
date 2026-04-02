using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class AnalyteCode : ValueObject
{
    public string Value { get; }

    private AnalyteCode(string value) => Value = value;

    public static AnalyteCode Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new AnalyteCode(value.Trim().ToUpperInvariant());
    }
}
