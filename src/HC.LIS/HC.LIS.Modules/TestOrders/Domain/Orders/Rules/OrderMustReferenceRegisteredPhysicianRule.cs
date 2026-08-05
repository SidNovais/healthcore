using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Domain.Orders.Rules;

public class OrderMustReferenceRegisteredPhysicianException : BaseBusinessRuleException
{
    public OrderMustReferenceRegisteredPhysicianException() { }
    public OrderMustReferenceRegisteredPhysicianException(string message) : base(message) { }
    public OrderMustReferenceRegisteredPhysicianException(string message, System.Exception innerException) : base(message, innerException) { }
    public OrderMustReferenceRegisteredPhysicianException(IBusinessRule rule) : base(rule) { }
}

public class OrderMustReferenceRegisteredPhysicianRule(
    RequestingPhysician? requestingPhysician
) : IBusinessRule
{
    private readonly RequestingPhysician? _requestingPhysician = requestingPhysician;
    public bool IsBroken() => _requestingPhysician is null;
    public void ThrowException() => throw new OrderMustReferenceRegisteredPhysicianException(this);
    public string Message => "An order must reference a registered physician";
}
