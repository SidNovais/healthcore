using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

public class RecordResultForNonPendingWorklistItemException : BaseBusinessRuleException
{
    public RecordResultForNonPendingWorklistItemException(string message) : base(message) { }
    public RecordResultForNonPendingWorklistItemException(string message, System.Exception innerException) : base(message, innerException) { }
    public RecordResultForNonPendingWorklistItemException(IBusinessRule rule) : base(rule) { }
    public RecordResultForNonPendingWorklistItemException() { }
}

public class CannotRecordResultForNonPendingWorklistItemRule(
    string actualStatus
) : IBusinessRule
{
    private readonly string _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus != "Pending";
    public void ThrowException() => throw new RecordResultForNonPendingWorklistItemException(this);
    public string Message => "Cannot record result for a worklist item that is not Pending";
}
