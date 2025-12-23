using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPartiallyCompleteOrderItemWhenIsRejectedException : BaseBusinessRuleException
{
    public CannotPartiallyCompleteOrderItemWhenIsRejectedException(string message) : base(message)
    {
    }

    public CannotPartiallyCompleteOrderItemWhenIsRejectedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPartiallyCompleteOrderItemWhenIsRejectedException()
    {
    }
}
public class CannotPartiallyCompleteOrderItemWhenIsRejectedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsRejected;
    public void ThrowException() => throw new CannotPartiallyCompleteOrderItemWhenIsRejectedException();
    public string Message => "Order item cannot be partially complete when the order is rejected.";
}
