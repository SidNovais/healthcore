using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotAcceptOrderItemWhenIsCompletedException : BaseBusinessRuleException
{
    public CannotAcceptOrderItemWhenIsCompletedException(string message) : base(message)
    {
    }

    public CannotAcceptOrderItemWhenIsCompletedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotAcceptOrderItemWhenIsCompletedException()
    {
    }
}
public class CannotAcceptOrderItemWhenIsCompletedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsAccepted;
    public void ThrowException() => throw new CannotAcceptOrderItemWhenIsCompletedException();
    public string Message => "Order item cannot be accepted when the order is completed.";
}
