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
    WorklistItemStatus actualStatus
) : IBusinessRule
{
    private readonly WorklistItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => !_actualStatus.IsPending;
    public void ThrowException() => throw new RecordResultForNonPendingWorklistItemException(this);
    public string Message => "Cannot record result for a worklist item that is not Pending";
}
