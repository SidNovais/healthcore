using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CreateBarcodeBeforePatientIsWaitingException : BaseBusinessRuleException
{
    public CreateBarcodeBeforePatientIsWaitingException(string message) : base(message) { }
    public CreateBarcodeBeforePatientIsWaitingException(string message, System.Exception innerException) : base(message, innerException) { }
    public CreateBarcodeBeforePatientIsWaitingException(IBusinessRule rule) : base(rule) { }
    public CreateBarcodeBeforePatientIsWaitingException() { }
}

public class CannotCreateBarcodeBeforePatientIsWaitingRule(
    CollectionStatus status
) : IBusinessRule
{
    private readonly CollectionStatus _status = status;
    public bool IsBroken() => _status.IsArrived;
    public void ThrowException() => throw new CreateBarcodeBeforePatientIsWaitingException(this);
    public string Message => "Cannot create barcode before patient is waiting or called";
}
