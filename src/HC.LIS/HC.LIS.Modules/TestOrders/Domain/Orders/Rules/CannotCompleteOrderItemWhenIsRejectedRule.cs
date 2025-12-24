using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCompleteOrderItemWhenIsRejectedException : BaseBusinessRuleException
{
    public CannotCompleteOrderItemWhenIsRejectedException(string message) : base(message)
    {
    }

    public CannotCompleteOrderItemWhenIsRejectedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCompleteOrderItemWhenIsRejectedException()
    {
    }
}
public class CannotCompleteOrderItemWhenIsRejectedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsRejected;
    public void ThrowException() => throw new CannotCompleteOrderItemWhenIsRejectedException();
    public string Message => "Order item cannot be complete when order is rejected";
}
