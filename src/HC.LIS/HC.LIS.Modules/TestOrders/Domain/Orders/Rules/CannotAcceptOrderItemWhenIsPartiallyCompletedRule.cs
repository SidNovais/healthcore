using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotAcceptOrderItemWhenIsPartiallyCompletedException : BaseBusinessRuleException
{
    public CannotAcceptOrderItemWhenIsPartiallyCompletedException(string message) : base(message)
    {
    }

    public CannotAcceptOrderItemWhenIsPartiallyCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotAcceptOrderItemWhenIsPartiallyCompletedException()
    {
    }
}
public class CannotAcceptOrderItemWhenIsPartiallyCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotAcceptOrderItemWhenIsPartiallyCompletedException();
    public string Message => "Order item cannot be accepted when the order is partially completed.";
}
