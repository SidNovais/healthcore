using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class AnalysisResult : ValueObject
{
    public AnalyteCode AnalyteCode { get; }
    public string ResultValue { get; }
    public string ResultUnit { get; }
    public string ReferenceRange { get; }

    private AnalysisResult(
        AnalyteCode analyteCode,
        string resultValue,
        string resultUnit,
        string referenceRange)
    {
        AnalyteCode = analyteCode;
        ResultValue = resultValue;
        ResultUnit = resultUnit;
        ReferenceRange = referenceRange;
    }

    public static AnalysisResult Create(
        string analyteCode,
        string resultValue,
        string resultUnit,
        string referenceRange)
        => new(AnalyteCode.Of(analyteCode), resultValue, resultUnit, referenceRange);
}
