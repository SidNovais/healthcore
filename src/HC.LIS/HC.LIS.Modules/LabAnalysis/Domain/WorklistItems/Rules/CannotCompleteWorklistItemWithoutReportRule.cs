using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

public class CompleteWorklistItemWithoutReportException : BaseBusinessRuleException
{
    public CompleteWorklistItemWithoutReportException(string message) : base(message) { }
    public CompleteWorklistItemWithoutReportException(string message, System.Exception innerException) : base(message, innerException) { }
    public CompleteWorklistItemWithoutReportException(IBusinessRule rule) : base(rule) { }
    public CompleteWorklistItemWithoutReportException() { }
}

public class CannotCompleteWorklistItemWithoutReportRule(
    string actualStatus
) : IBusinessRule
{
    private readonly string _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus != "ReportGenerated";
    public void ThrowException() => throw new CompleteWorklistItemWithoutReportException(this);
    public string Message => "Cannot complete a worklist item without a generated report";
}
