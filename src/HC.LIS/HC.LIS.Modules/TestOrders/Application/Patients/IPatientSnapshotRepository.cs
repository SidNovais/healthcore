namespace HC.LIS.Modules.TestOrders.Application.Patients;

public interface IPatientSnapshotRepository
{
    /// <summary>Returns the patient's full name, or <c>null</c> if no snapshot is stored yet.</summary>
    Task<string?> GetFullNameByIdAsync(Guid patientId);

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
