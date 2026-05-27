using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class AnonymizePatientCommand(
    Guid patientId,
    DateTime anonymizedAt
) : CommandBase
{
    public Guid PatientId { get; } = patientId;
    public DateTime AnonymizedAt { get; } = anonymizedAt;
}
