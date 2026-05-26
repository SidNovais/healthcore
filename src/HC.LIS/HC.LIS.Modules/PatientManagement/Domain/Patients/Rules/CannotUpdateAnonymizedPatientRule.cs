using HC.Core.Domain;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients.Rules;

public class CannotUpdateAnonymizedPatientException : BaseBusinessRuleException
{
    public CannotUpdateAnonymizedPatientException() { }
    public CannotUpdateAnonymizedPatientException(string message) : base(message) { }
    public CannotUpdateAnonymizedPatientException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotUpdateAnonymizedPatientException(IBusinessRule rule) : base(rule) { }
}

public class CannotUpdateAnonymizedPatientRule(PatientStatus status) : IBusinessRule
{
    private readonly PatientStatus _status = status;
    public bool IsBroken() => _status == PatientStatus.Anonymized;
    public void ThrowException() => throw new CannotUpdateAnonymizedPatientException(this);
    public string Message => "An anonymized patient cannot be updated";
}
