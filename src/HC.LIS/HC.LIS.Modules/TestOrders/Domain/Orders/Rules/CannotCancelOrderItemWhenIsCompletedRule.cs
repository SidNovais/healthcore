using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCancelOrderItemWhenIsCompletedException : BaseBusinessRuleException
{
    public CannotCancelOrderItemWhenIsCompletedException(string message) : base(message)
    {
    }

    public CannotCancelOrderItemWhenIsCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCancelOrderItemWhenIsCompletedException()
    {
    }
}
public class CannotCancelOrderItemWhenIsCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCompleted;
    public void ThrowException() => throw new CannotCancelOrderItemWhenIsCompletedException();
    public string Message => "Order item cannot be canceled when order is completed";
}
