using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class CannotRejectOrderItemWhenIsAcceptedException : BaseBusinessRuleException
{
    public CannotRejectOrderItemWhenIsAcceptedException(string message) : base(message)
    {
    }

    public CannotRejectOrderItemWhenIsAcceptedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public CannotRejectOrderItemWhenIsAcceptedException()
    {
    }
}
public class CannotRejectOrderItemWhenIsAcceptedRule(
    OrderItemStatus actualStatus
) : IBusinessRule
{
    private readonly OrderItemStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsAccepted;
    public void ThrowException() => throw new CannotRejectOrderItemWhenIsAcceptedException();
    public string Message => "Order item cannot be reject when the order is accepted.";
}
