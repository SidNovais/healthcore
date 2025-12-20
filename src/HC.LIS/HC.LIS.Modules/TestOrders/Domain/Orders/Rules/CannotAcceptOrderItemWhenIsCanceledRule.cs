using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotAcceptOrderItemWhenIsCanceled : BaseBusinessRuleException
{
    public CannotAcceptOrderItemWhenIsCanceled(string message) : base(message)
    {
    }

    public CannotAcceptOrderItemWhenIsCanceled(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotAcceptOrderItemWhenIsCanceled()
    {
    }
}
public class CannotAcceptOrderItemWhenIsCanceledRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsCanceled;
    public void ThrowException() => throw new CannotAcceptOrderItemWhenIsCanceled();
    public string Message => "Order item cannot be accepted when the order is canceled.";
}
