using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class AcceptOrderItemThanMoreOnceException : BaseBusinessRuleException
{
    public AcceptOrderItemThanMoreOnceException(string message) : base(message)
    {
    }

    public AcceptOrderItemThanMoreOnceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public AcceptOrderItemThanMoreOnceException()
    {
    }
}
public class CannotAcceptOrderItemThanMoreOnceRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsAccepted;
    public void ThrowException() => throw new AcceptOrderItemThanMoreOnceException();
    public string Message => "Order item cannot be accepted than more once";
}
