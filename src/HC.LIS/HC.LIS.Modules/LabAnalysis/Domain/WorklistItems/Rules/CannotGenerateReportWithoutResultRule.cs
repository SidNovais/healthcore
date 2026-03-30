using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

public class GenerateReportWithoutResultException : BaseBusinessRuleException
{
    public GenerateReportWithoutResultException(string message) : base(message) { }
    public GenerateReportWithoutResultException(string message, System.Exception innerException) : base(message, innerException) { }
    public GenerateReportWithoutResultException(IBusinessRule rule) : base(rule) { }
    public GenerateReportWithoutResultException() { }
}

public class CannotGenerateReportWithoutResultRule(
    WorklistItemStatus actualStatus
) : IBusinessRule
{
    private readonly WorklistItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => !_actualStatus.IsResultReceived;
    public void ThrowException() => throw new GenerateReportWithoutResultException(this);
    public string Message => "Cannot generate report without a recorded result";
}
