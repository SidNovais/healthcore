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
    string actualStatus
) : IBusinessRule
{
    private readonly string _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus != "ResultReceived";
    public void ThrowException() => throw new GenerateReportWithoutResultException(this);
    public string Message => "Cannot generate report without a recorded result";
}
