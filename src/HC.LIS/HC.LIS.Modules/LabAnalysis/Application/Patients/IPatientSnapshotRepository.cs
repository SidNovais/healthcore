namespace HC.LIS.Modules.LabAnalysis.Application.Patients;

public interface IPatientSnapshotRepository
{
    Task StoreAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email,
        DateTime registeredAt);

    Task UpdateAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email);

    Task AnonymizeAsync(Guid patientId, DateTime anonymizedAt);
}
