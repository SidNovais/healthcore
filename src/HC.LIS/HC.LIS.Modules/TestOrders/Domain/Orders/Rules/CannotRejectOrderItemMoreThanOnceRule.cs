using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotRejectOrderItemMoreThanOnceException : BaseBusinessRuleException
{
    public CannotRejectOrderItemMoreThanOnceException()
    {
    }
    public CannotRejectOrderItemMoreThanOnceException(string message) : base(message)
    {
    }

    public CannotRejectOrderItemMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotRejectOrderItemMoreThanOnceException(IBusinessRule rule) : base(rule)
    {
    }

}
public class CannotRejectOrderItemMoreThanOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsRejected;
    public void ThrowException() => throw new CannotRejectOrderItemMoreThanOnceException(this);
    public string Message => "Order item cannot be reject than more once";
}
