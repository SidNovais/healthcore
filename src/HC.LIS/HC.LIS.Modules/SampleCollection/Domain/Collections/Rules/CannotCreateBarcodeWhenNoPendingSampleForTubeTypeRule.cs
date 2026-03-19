using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CreateBarcodeWhenNoPendingSampleForTubeTypeException : BaseBusinessRuleException
{
    public CreateBarcodeWhenNoPendingSampleForTubeTypeException(string message) : base(message) { }
    public CreateBarcodeWhenNoPendingSampleForTubeTypeException(string message, System.Exception innerException) : base(message, innerException) { }
    public CreateBarcodeWhenNoPendingSampleForTubeTypeException(IBusinessRule rule) : base(rule) { }
    public CreateBarcodeWhenNoPendingSampleForTubeTypeException() { }
}

public class CannotCreateBarcodeWhenNoPendingSampleForTubeTypeRule(
    bool hasPendingSample
) : IBusinessRule
{
    private readonly bool _hasPendingSample = hasPendingSample;
    public bool IsBroken() => !_hasPendingSample;
    public void ThrowException() => throw new CreateBarcodeWhenNoPendingSampleForTubeTypeException(this);
    public string Message => "Cannot create a barcode when no pending sample exists for the given tube type";
}
