using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class OrderMustReferenceActivePhysicianException : BaseBusinessRuleException
{
    public OrderMustReferenceActivePhysicianException() { }
    public OrderMustReferenceActivePhysicianException(string message) : base(message) { }
    public OrderMustReferenceActivePhysicianException(string message, System.Exception innerException) : base(message, innerException) { }
    public OrderMustReferenceActivePhysicianException(IBusinessRule rule) : base(rule) { }
}

public class OrderMustReferenceActivePhysicianRule(
    RequestingPhysician? requestingPhysician
) : IBusinessRule
{
    private readonly RequestingPhysician? _requestingPhysician = requestingPhysician;
    public bool IsBroken() => _requestingPhysician is { IsActive: false };
    public void ThrowException() => throw new OrderMustReferenceActivePhysicianException(this);
    public string Message => "An order must reference an active physician";
}
