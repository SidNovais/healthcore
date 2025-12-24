using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotRejectOrderItemWhenIsCompletedException : BaseBusinessRuleException
{
    public CannotRejectOrderItemWhenIsCompletedException(string message) : base(message)
    {
    }

    public CannotRejectOrderItemWhenIsCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotRejectOrderItemWhenIsCompletedException()
    {
    }
}
public class CannotRejectOrderItemWhenIsCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCompleted;
    public void ThrowException() => throw new CannotRejectOrderItemWhenIsCompletedException();
    public string Message => "Order item cannot be reject when the order is completed.";
}
