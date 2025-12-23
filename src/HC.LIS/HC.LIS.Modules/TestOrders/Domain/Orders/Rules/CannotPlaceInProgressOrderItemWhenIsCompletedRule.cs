using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceInProgressOrderItemWhenIsCompletedException : BaseBusinessRuleException
{
    public CannotPlaceInProgressOrderItemWhenIsCompletedException(string message) : base(message)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsCompletedException()
    {
    }
}
public class CannotPlaceInProgressOrderItemWhenIsCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCompleted;
    public void ThrowException() => throw new CannotPlaceInProgressOrderItemWhenIsCompletedException();
    public string Message => "Order item cannot be place in progress when the order is completed.";
}
