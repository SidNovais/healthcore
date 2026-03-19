using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class MoveToWaitingWhenNotArrivedException : BaseBusinessRuleException
{
    public MoveToWaitingWhenNotArrivedException(string message) : base(message) { }
    public MoveToWaitingWhenNotArrivedException(string message, System.Exception innerException) : base(message, innerException) { }
    public MoveToWaitingWhenNotArrivedException(IBusinessRule rule) : base(rule) { }
    public MoveToWaitingWhenNotArrivedException() { }
}

public class CannotMoveToWaitingWhenNotArrivedRule(
    CollectionStatus status
) : IBusinessRule
{
    private readonly CollectionStatus _status = status;
    public bool IsBroken() => !_status.IsArrived;
    public void ThrowException() => throw new MoveToWaitingWhenNotArrivedException(this);
    public string Message => "Cannot move to waiting when patient has not arrived";
}
