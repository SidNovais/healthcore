using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Rules;

public class CannotDispatchInfoForNonAwaitingQuerySampleException : BaseBusinessRuleException
{
    public CannotDispatchInfoForNonAwaitingQuerySampleException() { }
    public CannotDispatchInfoForNonAwaitingQuerySampleException(string message) : base(message) { }
    public CannotDispatchInfoForNonAwaitingQuerySampleException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotDispatchInfoForNonAwaitingQuerySampleException(IBusinessRule rule) : base(rule) { }
}

public class CannotDispatchInfoForNonAwaitingQuerySampleRule(
    AnalyzerSampleStatus status
) : IBusinessRule
{
    private readonly AnalyzerSampleStatus _status = status;
    public bool IsBroken() => !_status.IsAwaitingQuery;
    public void ThrowException() => throw new CannotDispatchInfoForNonAwaitingQuerySampleException(this);
    public string Message => "Cannot dispatch info for a sample that is not awaiting query";
}
