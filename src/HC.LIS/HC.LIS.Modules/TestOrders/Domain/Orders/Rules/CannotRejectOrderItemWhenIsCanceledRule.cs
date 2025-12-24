using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotRejectOrderItemWhenIsCanceledException : BaseBusinessRuleException
{
    public CannotRejectOrderItemWhenIsCanceledException(string message) : base(message)
    {
    }

    public CannotRejectOrderItemWhenIsCanceledException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotRejectOrderItemWhenIsCanceledException()
    {
    }
}
public class CannotRejectOrderItemWhenIsCanceledRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotRejectOrderItemWhenIsCanceledException();
    public string Message => "Order item cannot be reject when the order is canceled.";
}
