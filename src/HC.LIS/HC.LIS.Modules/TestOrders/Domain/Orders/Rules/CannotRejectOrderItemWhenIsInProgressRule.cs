using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotRejectOrderItemWhenIsInProgressException : BaseBusinessRuleException
{
    public CannotRejectOrderItemWhenIsInProgressException(string message) : base(message)
    {
    }

    public CannotRejectOrderItemWhenIsInProgressException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotRejectOrderItemWhenIsInProgressException()
    {
    }
}
public class CannotRejectOrderItemWhenIsInProgressRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsInProgress;
    public void ThrowException() => throw new CannotRejectOrderItemWhenIsInProgressException();
    public string Message => "Order item cannot be reject when the order is in progress.";
}
