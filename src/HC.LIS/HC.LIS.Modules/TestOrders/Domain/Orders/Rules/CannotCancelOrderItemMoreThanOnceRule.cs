using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCancelOrderItemMoreThanOnceException : BaseBusinessRuleException
{
    public CannotCancelOrderItemMoreThanOnceException(string message) : base(message)
    {
    }

    public CannotCancelOrderItemMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCancelOrderItemMoreThanOnceException()
    {
    }
}
public class CannotCancelOrderItemMoreThanOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotCancelOrderItemMoreThanOnceException();
    public string Message => "Order item cannot be canceled more than once";
}
