using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceOnHoldOrderItemThanMoreOnceException : BaseBusinessRuleException
{
    public CannotPlaceOnHoldOrderItemThanMoreOnceException(string message) : base(message)
    {
    }

    public CannotPlaceOnHoldOrderItemThanMoreOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceOnHoldOrderItemThanMoreOnceException(IBusinessRule rule) : base(rule)
    {
    }
    public CannotPlaceOnHoldOrderItemThanMoreOnceException()
    {
    }
}
public class CannotPlaceOnHoldOrderItemThanMoreOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsOnHold;
    public void ThrowException() => throw new CannotPlaceOnHoldOrderItemThanMoreOnceException(this);
    public string Message => "Order item cannot be place on hold than more once";
}
