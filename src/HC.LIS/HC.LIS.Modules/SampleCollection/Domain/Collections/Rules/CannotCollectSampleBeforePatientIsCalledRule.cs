using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CollectSampleBeforePatientIsCalledException : BaseBusinessRuleException
{
    public CollectSampleBeforePatientIsCalledException(string message) : base(message) { }
    public CollectSampleBeforePatientIsCalledException(string message, System.Exception innerException) : base(message, innerException) { }
    public CollectSampleBeforePatientIsCalledException(IBusinessRule rule) : base(rule) { }
    public CollectSampleBeforePatientIsCalledException() { }
}

public class CannotCollectSampleBeforePatientIsCalledRule(
    CollectionStatus status
) : IBusinessRule
{
    private readonly CollectionStatus _status = status;
    public bool IsBroken() => !_status.IsCalled;
    public void ThrowException() => throw new CollectSampleBeforePatientIsCalledException(this);
    public string Message => "Cannot collect sample before patient is called";
}
