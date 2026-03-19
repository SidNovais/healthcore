using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class ExamPreparationNotVerifiedException : BaseBusinessRuleException
{
    public ExamPreparationNotVerifiedException(string message) : base(message) { }
    public ExamPreparationNotVerifiedException(string message, System.Exception innerException) : base(message, innerException) { }
    public ExamPreparationNotVerifiedException(IBusinessRule rule) : base(rule) { }
    public ExamPreparationNotVerifiedException() { }
}

public class ExamPreparationMustBeVerifiedToAdvanceRule(
    bool examPreparationVerified
) : IBusinessRule
{
    private readonly bool _examPreparationVerified = examPreparationVerified;
    public bool IsBroken() => !_examPreparationVerified;
    public void ThrowException() => throw new ExamPreparationNotVerifiedException(this);
    public string Message => "Exam preparation must be verified before moving to waiting";
}
