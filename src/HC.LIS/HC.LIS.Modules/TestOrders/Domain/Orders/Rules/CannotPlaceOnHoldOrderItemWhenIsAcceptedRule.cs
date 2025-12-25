using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceOnHoldOrderItemWhenIsAcceptedException : BaseBusinessRuleException
{
    public CannotPlaceOnHoldOrderItemWhenIsAcceptedException(string message) : base(message)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsAcceptedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceOnHoldOrderItemWhenIsAcceptedException()
    {
    }
}
public class CannotPlaceOnHoldOrderItemWhenIsAcceptedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsAccepted;
    public void ThrowException() => throw new CannotPlaceOnHoldOrderItemWhenIsAcceptedException();
    public string Message => "Order item cannot be place on hold when the order is accepted.";
}
