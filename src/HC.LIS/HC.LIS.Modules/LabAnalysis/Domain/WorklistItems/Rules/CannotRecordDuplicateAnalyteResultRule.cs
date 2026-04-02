using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

public class DuplicateAnalyteResultException : BaseBusinessRuleException
{
    public DuplicateAnalyteResultException() { }
    public DuplicateAnalyteResultException(string message) : base(message) { }
    public DuplicateAnalyteResultException(string message, System.Exception innerException) : base(message, innerException) { }
    public DuplicateAnalyteResultException(IBusinessRule rule) : base(rule) { }
}

public class CannotRecordDuplicateAnalyteResultRule(
    IReadOnlyCollection<AnalysisResult> existingResults,
    AnalyteCode incomingCode
) : IBusinessRule
{
    private readonly IReadOnlyCollection<AnalysisResult> _existingResults = existingResults;
    private readonly AnalyteCode _incomingCode = incomingCode;

    public bool IsBroken() => _existingResults.Any(r => r.AnalyteCode == _incomingCode);
    public void ThrowException() => throw new DuplicateAnalyteResultException(this);
    public string Message => $"A result for analyte '{_incomingCode.Value}' has already been recorded for this worklist item";
}
