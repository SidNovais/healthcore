using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPartiallyCompleteOrderItemMoreThanOnceException : BaseBusinessRuleException
{
    public CannotPartiallyCompleteOrderItemMoreThanOnceException(string message) : base(message)
    {
    }

    public CannotPartiallyCompleteOrderItemMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPartiallyCompleteOrderItemMoreThanOnceException(IBusinessRule rule) : base(rule)
    {
    }
    public CannotPartiallyCompleteOrderItemMoreThanOnceException()
    {
    }
}
public class CannotPartiallyCompleteOrderItemMoreThanOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotPartiallyCompleteOrderItemMoreThanOnceException(this);
    public string Message => "Order item cannot be partially complete than more once";
}
