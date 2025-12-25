using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceOnHoldOrderItemWhenIsPartiallyCompletedException : BaseBusinessRuleException
{
    public CannotPlaceOnHoldOrderItemWhenIsPartiallyCompletedException(string message) : base(message)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsPartiallyCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsPartiallyCompletedException()
    {
    }
}
public class CannotPlaceOnHoldOrderItemWhenIsPartiallyCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotPlaceOnHoldOrderItemWhenIsPartiallyCompletedException();
    public string Message => "Order item cannot be place on hold when the order is partially completed.";
}
