using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CollectSampleWithoutBarcodeException : BaseBusinessRuleException
{
    public CollectSampleWithoutBarcodeException(string message) : base(message) { }
    public CollectSampleWithoutBarcodeException(string message, System.Exception innerException) : base(message, innerException) { }
    public CollectSampleWithoutBarcodeException(IBusinessRule rule) : base(rule) { }
    public CollectSampleWithoutBarcodeException() { }
}

public class CannotCollectSampleWithoutBarcodeRule(
    bool hasBarcode
) : IBusinessRule
{
    private readonly bool _hasBarcode = hasBarcode;
    public bool IsBroken() => !_hasBarcode;
    public void ThrowException() => throw new CollectSampleWithoutBarcodeException(this);
    public string Message => "Cannot collect sample without a barcode assigned";
}
