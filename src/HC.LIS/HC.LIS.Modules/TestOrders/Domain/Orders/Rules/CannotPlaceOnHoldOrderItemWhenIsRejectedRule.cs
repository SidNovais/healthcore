using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceOnHoldOrderItemWhenIsRejectedException : BaseBusinessRuleException
{
    public CannotPlaceOnHoldOrderItemWhenIsRejectedException(string message) : base(message)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsRejectedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsRejectedException()
    {
    }
}
public class CannotPlaceOnHoldOrderItemWhenIsRejectedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsRejected;
    public void ThrowException() => throw new CannotPlaceOnHoldOrderItemWhenIsRejectedException();
    public string Message => "Order item cannot be place on hold when the order is rejected.";
}
