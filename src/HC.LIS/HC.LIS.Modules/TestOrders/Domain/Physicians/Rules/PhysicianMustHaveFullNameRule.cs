using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Rules;

public class PhysicianMustHaveFullNameException : BaseBusinessRuleException
{
    public PhysicianMustHaveFullNameException() { }
    public PhysicianMustHaveFullNameException(string message) : base(message) { }
    public PhysicianMustHaveFullNameException(string message, System.Exception innerException) : base(message, innerException) { }
    public PhysicianMustHaveFullNameException(IBusinessRule rule) : base(rule) { }
}

public class PhysicianMustHaveFullNameRule(string? fullName) : IBusinessRule
{
    private readonly string? _fullName = fullName;
    public bool IsBroken() => string.IsNullOrWhiteSpace(_fullName);
    public void ThrowException() => throw new PhysicianMustHaveFullNameException(this);
    public string Message => "A physician must have a full name";
}
