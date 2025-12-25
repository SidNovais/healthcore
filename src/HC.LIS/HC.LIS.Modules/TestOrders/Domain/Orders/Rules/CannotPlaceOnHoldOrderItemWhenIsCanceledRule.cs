using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceOnHoldOrderItemWhenIsCanceledException : BaseBusinessRuleException
{
    public CannotPlaceOnHoldOrderItemWhenIsCanceledException(string message) : base(message)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsCanceledException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsCanceledException()
    {
    }
}
public class CannotPlaceOnHoldOrderItemWhenIsCanceledRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotPlaceOnHoldOrderItemWhenIsCanceledException();
    public string Message => "Order item cannot be place on hold when the order is canceled.";
}
