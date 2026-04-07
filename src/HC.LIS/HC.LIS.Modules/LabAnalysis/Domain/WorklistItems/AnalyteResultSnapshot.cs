namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class AnalyteResultSnapshot(
    string analyteCode,
    string resultValue,
    string resultUnit,
    string referenceRange,
    bool isOutOfRange)
{
    public string AnalyteCode { get; } = analyteCode;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public bool IsOutOfRange { get; } = isOutOfRange;
}
