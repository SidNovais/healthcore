using System;

namespace HC.Core.Domain;

public class BaseBusinessRuleException : Exception
{
    public BaseBusinessRuleException() { }

    public BaseBusinessRuleException(string message) : base(message)
    {
    }

    public BaseBusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public BaseBusinessRuleException(IBusinessRule rule) : base(PassThroughNonNull(rule).Message)
    {
        ArgumentNullException.ThrowIfNull(rule, "IBusiness rule cannot be null");
        Rule = rule;
    }

    private static IBusinessRule PassThroughNonNull(IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule, "IBusiness rule cannot be null");
        return rule;
    }

    public IBusinessRule? Rule { get; }
}
