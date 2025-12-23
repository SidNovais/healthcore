using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotPartiallyCompleteOrderItemWhenIsCanceledException : BaseBusinessRuleException
{
    public CannotPartiallyCompleteOrderItemWhenIsCanceledException(string message) : base(message)
    {
    }

    public CannotPartiallyCompleteOrderItemWhenIsCanceledException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotPartiallyCompleteOrderItemWhenIsCanceledException()
    {
    }
}
public class CannotPartiallyCompleteOrderItemWhenIsCanceledRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotPartiallyCompleteOrderItemWhenIsCanceledException();
    public string Message => "Order item cannot be partially complete when the order is canceled.";
}
