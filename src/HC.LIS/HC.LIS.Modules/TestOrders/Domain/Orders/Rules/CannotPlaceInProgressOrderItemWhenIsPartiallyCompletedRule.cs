using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceInProgressOrderItemWhenIsPartiallyCompletedException : BaseBusinessRuleException
{
    public CannotPlaceInProgressOrderItemWhenIsPartiallyCompletedException(string message) : base(message)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsPartiallyCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsPartiallyCompletedException()
    {
    }
}
public class CannotPlaceInProgressOrderItemWhenIsPartiallyCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotPlaceInProgressOrderItemWhenIsPartiallyCompletedException();
    public string Message => "Order item cannot be place in progress when the order is partially completed.";
}
