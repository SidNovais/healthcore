using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceInProgressOrderItemMoreThanOnceException : BaseBusinessRuleException
{
    public CannotPlaceInProgressOrderItemMoreThanOnceException(string message) : base(message)
    {
    }

    public CannotPlaceInProgressOrderItemMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceInProgressOrderItemMoreThanOnceException()
    {
    }
}
public class CannotPlaceInProgressOrderItemMoreThanOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsInProgress;
    public void ThrowException() => throw new CannotPlaceInProgressOrderItemMoreThanOnceException();
    public string Message => "Order item cannot be place in progress more than once";
}
