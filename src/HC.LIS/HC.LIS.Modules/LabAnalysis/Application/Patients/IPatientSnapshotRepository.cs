namespace HC.LIS.Modules.LabAnalysis.Application.Patients;

/// <summary>The display fields a worklist row needs from a patient snapshot.</summary>
public record PatientSnapshotView(string FullName, DateTime DateOfBirth, string? Gender);

public interface IPatientSnapshotRepository
{
    /// <summary>Returns the display snapshot for a patient, or <c>null</c> if none is stored yet.</summary>
    Task<PatientSnapshotView?> GetByIdAsync(Guid patientId);

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
