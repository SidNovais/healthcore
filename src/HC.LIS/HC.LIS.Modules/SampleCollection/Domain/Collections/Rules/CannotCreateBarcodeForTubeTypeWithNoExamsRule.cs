using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CreateBarcodeForTubeTypeWithNoExamsException : BaseBusinessRuleException
{
    public CreateBarcodeForTubeTypeWithNoExamsException(string message) : base(message) { }
    public CreateBarcodeForTubeTypeWithNoExamsException(string message, System.Exception innerException) : base(message, innerException) { }
    public CreateBarcodeForTubeTypeWithNoExamsException(IBusinessRule rule) : base(rule) { }
    public CreateBarcodeForTubeTypeWithNoExamsException() { }
}

public class CannotCreateBarcodeForTubeTypeWithNoExamsRule(
    bool hasMatchingExams
) : IBusinessRule
{
    private readonly bool _hasMatchingExams = hasMatchingExams;
    public bool IsBroken() => !_hasMatchingExams;
    public void ThrowException() => throw new CreateBarcodeForTubeTypeWithNoExamsException(this);
    public string Message => "Cannot create a barcode for a tube type with no matching exams";
}
