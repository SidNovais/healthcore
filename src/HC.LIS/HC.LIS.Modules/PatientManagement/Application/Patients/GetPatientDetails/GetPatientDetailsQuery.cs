using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;

public class GetPatientDetailsQuery(Guid patientId) : QueryBase<PatientDetailsDto?>
{
    public Guid PatientId { get; } = patientId;
}
