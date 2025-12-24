using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotCompleteOrderItemWhenIsCanceledException : BaseBusinessRuleException
{
    public CannotCompleteOrderItemWhenIsCanceledException(string message) : base(message)
    {
    }

    public CannotCompleteOrderItemWhenIsCanceledException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotCompleteOrderItemWhenIsCanceledException()
    {
    }
}
public class CannotCompleteOrderItemWhenIsCanceledRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotCompleteOrderItemWhenIsCanceledException();
    public string Message => "Order item cannot be canceled when order is canceled";
}
