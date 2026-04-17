using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users.Rules;

public class CannotActivateAlreadyActiveUserException : BaseBusinessRuleException
{
    public CannotActivateAlreadyActiveUserException() { }
    public CannotActivateAlreadyActiveUserException(string message) : base(message) { }
    public CannotActivateAlreadyActiveUserException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotActivateAlreadyActiveUserException(IBusinessRule rule) : base(rule) { }
}

public class CannotActivateAlreadyActiveUserRule(UserStatus status) : IBusinessRule
{
    public bool IsBroken() => status.IsActive;
    public void ThrowException() => throw new CannotActivateAlreadyActiveUserException(this);
    public string Message => "User cannot be activated because they are already active";
}
