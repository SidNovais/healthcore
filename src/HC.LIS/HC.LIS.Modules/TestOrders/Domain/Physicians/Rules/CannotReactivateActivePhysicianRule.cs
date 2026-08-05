using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Rules;

public class CannotReactivateActivePhysicianException : BaseBusinessRuleException
{
    public CannotReactivateActivePhysicianException() { }
    public CannotReactivateActivePhysicianException(string message) : base(message) { }
    public CannotReactivateActivePhysicianException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotReactivateActivePhysicianException(IBusinessRule rule) : base(rule) { }
}

public class CannotReactivateActivePhysicianRule(PhysicianStatus status) : IBusinessRule
{
    private readonly PhysicianStatus _status = status;
    public bool IsBroken() => _status == PhysicianStatus.Active;
    public void ThrowException() => throw new CannotReactivateActivePhysicianException(this);
    public string Message => "A physician that is already active cannot be reactivated";
}
