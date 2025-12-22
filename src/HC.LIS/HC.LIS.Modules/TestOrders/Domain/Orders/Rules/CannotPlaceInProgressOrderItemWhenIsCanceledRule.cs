using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPlaceInProgressOrderItemWhenIsCanceledException : BaseBusinessRuleException
{
    public CannotPlaceInProgressOrderItemWhenIsCanceledException(string message) : base(message)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsCanceledException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPlaceInProgressOrderItemWhenIsCanceledException()
    {
    }
}
public class CannotPlaceInProgressOrderItemWhenIsCanceledRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotPlaceInProgressOrderItemWhenIsCanceledException();
    public string Message => "Order item cannot be place in progress when the order is canceled.";
}
