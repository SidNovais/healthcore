using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotAcceptOrderItemWhenIsRejectedException : BaseBusinessRuleException
{
    public CannotAcceptOrderItemWhenIsRejectedException(string message) : base(message)
    {
    }

    public CannotAcceptOrderItemWhenIsRejectedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotAcceptOrderItemWhenIsRejectedException()
    {
    }
}
public class CannotAcceptOrderItemWhenIsRejectedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotAcceptOrderItemWhenIsRejectedException();
    public string Message => "Order item cannot be accepted when the order is rejected.";
}
