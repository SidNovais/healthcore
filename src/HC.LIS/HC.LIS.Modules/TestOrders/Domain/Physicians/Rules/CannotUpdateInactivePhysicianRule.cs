using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Rules;

public class CannotUpdateInactivePhysicianException : BaseBusinessRuleException
{
    public CannotUpdateInactivePhysicianException() { }
    public CannotUpdateInactivePhysicianException(string message) : base(message) { }
    public CannotUpdateInactivePhysicianException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotUpdateInactivePhysicianException(IBusinessRule rule) : base(rule) { }
}

public class CannotUpdateInactivePhysicianRule(PhysicianStatus status) : IBusinessRule
{
    private readonly PhysicianStatus _status = status;
    public bool IsBroken() => _status == PhysicianStatus.Inactive;
    public void ThrowException() => throw new CannotUpdateInactivePhysicianException(this);
    public string Message => "An inactive physician cannot be updated";
}
