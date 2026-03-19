using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CollectSampleMoreThanOnceException : BaseBusinessRuleException
{
    public CollectSampleMoreThanOnceException(string message) : base(message) { }
    public CollectSampleMoreThanOnceException(string message, System.Exception innerException) : base(message, innerException) { }
    public CollectSampleMoreThanOnceException(IBusinessRule rule) : base(rule) { }
    public CollectSampleMoreThanOnceException() { }
}

public class CannotCollectSampleMoreThanOnceRule(
    SampleStatus status
) : IBusinessRule
{
    private readonly SampleStatus _status = status;
    public bool IsBroken() => _status.IsCollected;
    public void ThrowException() => throw new CollectSampleMoreThanOnceException(this);
    public string Message => "Cannot collect sample more than once";
}
