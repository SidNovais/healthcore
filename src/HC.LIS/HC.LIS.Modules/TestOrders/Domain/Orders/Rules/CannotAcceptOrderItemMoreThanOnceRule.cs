using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class AcceptOrderItemMoreThanOnceException : BaseBusinessRuleException
{
    public AcceptOrderItemMoreThanOnceException(string message) : base(message)
    {
    }

    public AcceptOrderItemMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public AcceptOrderItemMoreThanOnceException(IBusinessRule rule) : base(rule)
    {
    }
    public AcceptOrderItemMoreThanOnceException()
    {
    }
}
public class CannotAcceptOrderItemMoreThanOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsAccepted;
    public void ThrowException() => throw new AcceptOrderItemMoreThanOnceException(this);
    public string Message => "Order item cannot be accepted more than once";
}
