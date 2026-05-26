using HC.Core.Domain;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients.Rules;

public class CannotAnonymizeAlreadyAnonymizedPatientException : BaseBusinessRuleException
{
    public CannotAnonymizeAlreadyAnonymizedPatientException() { }
    public CannotAnonymizeAlreadyAnonymizedPatientException(string message) : base(message) { }
    public CannotAnonymizeAlreadyAnonymizedPatientException(string message, System.Exception innerException) : base(message, innerException) { }
    public CannotAnonymizeAlreadyAnonymizedPatientException(IBusinessRule rule) : base(rule) { }
}

public class CannotAnonymizeAlreadyAnonymizedPatientRule(PatientStatus status) : IBusinessRule
{
    private readonly PatientStatus _status = status;
    public bool IsBroken() => _status == PatientStatus.Anonymized;
    public void ThrowException() => throw new CannotAnonymizeAlreadyAnonymizedPatientException(this);
    public string Message => "A patient that is already anonymized cannot be anonymized again";
}
