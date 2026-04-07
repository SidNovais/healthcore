using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

public class CannotSignReportWhenNotInReportGeneratedStatusException : BaseBusinessRuleException
{
    public CannotSignReportWhenNotInReportGeneratedStatusException(string message) : base(message) { }
    public CannotSignReportWhenNotInReportGeneratedStatusException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotSignReportWhenNotInReportGeneratedStatusException(IBusinessRule rule) : base(rule) { }
    public CannotSignReportWhenNotInReportGeneratedStatusException() { }
}

public class CannotSignReportWhenNotInReportGeneratedStatusRule(
    WorklistItemStatus actualStatus
) : IBusinessRule
{
    private readonly WorklistItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => !_actualStatus.IsReportGenerated;
    public void ThrowException() => throw new CannotSignReportWhenNotInReportGeneratedStatusException(this);
    public string Message => "Cannot sign report: worklist item must be in ReportGenerated status";
}
