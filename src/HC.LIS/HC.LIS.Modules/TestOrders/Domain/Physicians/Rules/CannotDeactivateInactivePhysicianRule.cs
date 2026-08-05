using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Rules;

public class CannotDeactivateInactivePhysicianException : BaseBusinessRuleException
{
    public CannotDeactivateInactivePhysicianException() { }
    public CannotDeactivateInactivePhysicianException(string message) : base(message) { }
    public CannotDeactivateInactivePhysicianException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotDeactivateInactivePhysicianException(IBusinessRule rule) : base(rule) { }
}

public class CannotDeactivateInactivePhysicianRule(PhysicianStatus status) : IBusinessRule
{
    private readonly PhysicianStatus _status = status;
    public bool IsBroken() => _status == PhysicianStatus.Inactive;
    public void ThrowException() => throw new CannotDeactivateInactivePhysicianException(this);
    public string Message => "A physician that is already inactive cannot be deactivated again";
}
