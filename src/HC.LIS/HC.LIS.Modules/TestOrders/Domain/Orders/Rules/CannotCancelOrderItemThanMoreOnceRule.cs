using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CancelOrderItemThanMoreOnceException : BaseBusinessRuleException
{
    public CancelOrderItemThanMoreOnceException(string message) : base(message)
    {
    }

    public CancelOrderItemThanMoreOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CancelOrderItemThanMoreOnceException()
    {
    }
}
public class CannotCancelOrderItemThanMoreOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CancelOrderItemThanMoreOnceException();
    public string Message => "Order item cannot be canceled than more once";
}
