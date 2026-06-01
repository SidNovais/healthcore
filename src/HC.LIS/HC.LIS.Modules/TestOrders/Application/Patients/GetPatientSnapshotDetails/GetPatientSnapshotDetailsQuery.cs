using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Patients.GetPatientSnapshotDetails;

public class GetPatientSnapshotDetailsQuery(Guid patientId) : QueryBase<PatientSnapshotDetailsDto?>
{
    public Guid PatientId { get; } = patientId;
}
