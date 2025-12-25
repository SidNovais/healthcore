using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceOnHoldOrderItemWhenIsCompletedException : BaseBusinessRuleException
{
    public CannotPlaceOnHoldOrderItemWhenIsCompletedException(string message) : base(message)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsCompletedException()
    {
    }
}
public class CannotPlaceOnHoldOrderItemWhenIsCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotPlaceOnHoldOrderItemWhenIsCompletedException();
    public string Message => "Order item cannot be place on hold when the order is completed.";
}
