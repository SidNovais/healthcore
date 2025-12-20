using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCancelOrderItemWhenIsRejectedException : BaseBusinessRuleException
{
    public CannotCancelOrderItemWhenIsRejectedException(string message) : base(message)
    {
    }

    public CannotCancelOrderItemWhenIsRejectedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCancelOrderItemWhenIsRejectedException()
    {
    }
}
public class CannotCancelOrderItemWhenIsRejectedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotCancelOrderItemWhenIsRejectedException();
    public string Message => "Order item cannot be canceled when order is rejected";
}
