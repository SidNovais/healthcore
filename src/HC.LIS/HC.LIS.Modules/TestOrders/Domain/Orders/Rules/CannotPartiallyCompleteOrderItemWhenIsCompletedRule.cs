using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPartiallyCompleteOrderItemWhenIsCompletedException : BaseBusinessRuleException
{
    public CannotPartiallyCompleteOrderItemWhenIsCompletedException(string message) : base(message)
    {
    }

    public CannotPartiallyCompleteOrderItemWhenIsCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPartiallyCompleteOrderItemWhenIsCompletedException()
    {
    }
}
public class CannotPartiallyCompleteOrderItemWhenIsCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCompleted;
    public void ThrowException() => throw new CannotPartiallyCompleteOrderItemWhenIsCompletedException();
    public string Message => "Order item cannot be partially complete when the order is completed.";
}
