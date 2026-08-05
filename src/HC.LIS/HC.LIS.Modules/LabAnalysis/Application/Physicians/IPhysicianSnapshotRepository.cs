namespace HC.LIS.Modules.LabAnalysis.Application.Physicians;

/// <summary>The display fields a worklist row needs from a physician snapshot.</summary>
public record PhysicianSnapshotView(string FullName, string? LicenceNumber);

public interface IPhysicianSnapshotRepository
{
    /// <summary>Returns the display snapshot for a physician, or <c>null</c> if none is stored yet.</summary>
    Task<PhysicianSnapshotView?> GetByIdAsync(Guid physicianId);

    Task StoreAsync(Guid physicianId, string fullName, string? licenceNumber, DateTime registeredAt);

    Task UpdateAsync(Guid physicianId, string fullName, string? licenceNumber, DateTime updatedAt);

    Task DeactivateAsync(Guid physicianId, DateTime deactivatedAt);

    Task ReactivateAsync(Guid physicianId, DateTime reactivatedAt);
}
