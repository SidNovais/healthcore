using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users.Rules;

public class CannotActivateWithInvalidTokenException : BaseBusinessRuleException
{
    public CannotActivateWithInvalidTokenException() { }
    public CannotActivateWithInvalidTokenException(string message) : base(message) { }
    public CannotActivateWithInvalidTokenException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotActivateWithInvalidTokenException(IBusinessRule rule) : base(rule) { }
}

public class CannotActivateWithInvalidTokenRule(
    string storedToken,
    string providedToken) : IBusinessRule
{
    public bool IsBroken() => storedToken != providedToken;
    public void ThrowException() => throw new CannotActivateWithInvalidTokenException(this);
    public string Message => "User cannot be activated with an invalid invitation token";
}
