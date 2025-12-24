using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotRejectOrderItemWhenIsPartiallyCompletedException : BaseBusinessRuleException
{
    public CannotRejectOrderItemWhenIsPartiallyCompletedException(string message) : base(message)
    {
    }

    public CannotRejectOrderItemWhenIsPartiallyCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotRejectOrderItemWhenIsPartiallyCompletedException()
    {
    }
}
public class CannotRejectOrderItemWhenIsPartiallyCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotRejectOrderItemWhenIsPartiallyCompletedException();
    public string Message => "Order item cannot be reject when the order is partially completed.";
}
