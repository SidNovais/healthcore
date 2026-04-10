using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Rules;

public class CannotReceiveResultForNonDispatchedSampleException : BaseBusinessRuleException
{
    public CannotReceiveResultForNonDispatchedSampleException() { }
    public CannotReceiveResultForNonDispatchedSampleException(string message) : base(message) { }
    public CannotReceiveResultForNonDispatchedSampleException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotReceiveResultForNonDispatchedSampleException(IBusinessRule rule) : base(rule) { }
}

public class CannotReceiveResultForNonDispatchedSampleRule(
    AnalyzerSampleStatus status
) : IBusinessRule
{
    private readonly AnalyzerSampleStatus _status = status;
    public bool IsBroken() => !_status.IsInfoDispatched;
    public void ThrowException() => throw new CannotReceiveResultForNonDispatchedSampleException(this);
    public string Message => "Cannot receive result for a sample that has not been dispatched";
}
