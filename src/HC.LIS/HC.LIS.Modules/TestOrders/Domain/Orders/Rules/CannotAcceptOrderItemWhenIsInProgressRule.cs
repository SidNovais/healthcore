using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotAcceptOrderItemWhenIsInProgressException : BaseBusinessRuleException
{
    public CannotAcceptOrderItemWhenIsInProgressException(string message) : base(message)
    {
    }

    public CannotAcceptOrderItemWhenIsInProgressException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotAcceptOrderItemWhenIsInProgressException()
    {
    }
}
public class CannotAcceptOrderItemWhenIsInProgressRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsInProgress;
    public void ThrowException() => throw new CannotAcceptOrderItemWhenIsInProgressException();
    public string Message => "Order item cannot be accepted when the order is in progress.";
}
