using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CallPatientWhenNotWaitingException : BaseBusinessRuleException
{
    public CallPatientWhenNotWaitingException(string message) : base(message) { }
    public CallPatientWhenNotWaitingException(string message, System.Exception innerException) : base(message, innerException) { }
    public CallPatientWhenNotWaitingException(IBusinessRule rule) : base(rule) { }
    public CallPatientWhenNotWaitingException() { }
}

public class CannotCallPatientWhenNotWaitingRule(
    CollectionStatus status
) : IBusinessRule
{
    private readonly CollectionStatus _status = status;
    public bool IsBroken() => !_status.IsWaiting;
    public void ThrowException() => throw new CallPatientWhenNotWaitingException(this);
    public string Message => "Cannot call patient when not in waiting status";
}
