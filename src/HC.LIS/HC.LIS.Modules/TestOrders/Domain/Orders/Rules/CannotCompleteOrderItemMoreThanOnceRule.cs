using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCompleteOrderItemMoreThanOnceException : BaseBusinessRuleException
{
    public CannotCompleteOrderItemMoreThanOnceException(string message) : base(message)
    {
    }

    public CannotCompleteOrderItemMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCompleteOrderItemMoreThanOnceException()
    {
    }
}
public class CannotCompleteOrderItemMoreThanOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotCompleteOrderItemMoreThanOnceException();
    public string Message => "Order item cannot be complete than more once";
}
