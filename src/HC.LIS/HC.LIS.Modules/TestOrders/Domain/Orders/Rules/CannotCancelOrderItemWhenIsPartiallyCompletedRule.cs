using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCancelOrderItemWhenIsPartiallyCompletedException : BaseBusinessRuleException
{
    public CannotCancelOrderItemWhenIsPartiallyCompletedException(string message) : base(message)
    {
    }

    public CannotCancelOrderItemWhenIsPartiallyCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCancelOrderItemWhenIsPartiallyCompletedException()
    {
    }
}
public class CannotCancelOrderItemWhenIsPartiallyCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotCancelOrderItemWhenIsPartiallyCompletedException();
    public string Message => "Order item cannot be canceled when order is partially completed";
}
