using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceInProgressOrderItemWhenIsRejectedException : BaseBusinessRuleException
{
    public CannotPlaceInProgressOrderItemWhenIsRejectedException(string message) : base(message)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsRejectedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsRejectedException()
    {
    }
}
public class CannotPlaceInProgressOrderItemWhenIsRejectedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPartiallyCompleted;
    public void ThrowException() => throw new CannotPlaceInProgressOrderItemWhenIsRejectedException();
    public string Message => "Order item cannot be place in progress when the order is rejected.";
}
