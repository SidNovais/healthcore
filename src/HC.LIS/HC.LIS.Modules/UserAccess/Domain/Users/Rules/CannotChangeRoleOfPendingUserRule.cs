using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users.Rules;

public class CannotChangeRoleOfPendingUserException : BaseBusinessRuleException
{
    public CannotChangeRoleOfPendingUserException() { }
    public CannotChangeRoleOfPendingUserException(string message) : base(message) { }
    public CannotChangeRoleOfPendingUserException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotChangeRoleOfPendingUserException(IBusinessRule rule) : base(rule) { }
}

public class CannotChangeRoleOfPendingUserRule(UserStatus status) : IBusinessRule
{
    public bool IsBroken() => status.IsPendingActivation;
    public void ThrowException() => throw new CannotChangeRoleOfPendingUserException(this);
    public string Message => "Cannot change the role of a user who has not yet activated their account";
}
