using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotAcceptOrderItemWhenIsInProgress : BaseBusinessRuleException
{
    public CannotAcceptOrderItemWhenIsInProgress(string message) : base(message)
    {
    }

    public CannotAcceptOrderItemWhenIsInProgress(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotAcceptOrderItemWhenIsInProgress()
    {
    }
}
public class CannotAcceptOrderItemWhenIsInProgressRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsInProgress;
    public void ThrowException() => throw new CannotAcceptOrderItemWhenIsInProgress();
    public string Message => "Order item cannot be accepted when the order is in progress.";
}
