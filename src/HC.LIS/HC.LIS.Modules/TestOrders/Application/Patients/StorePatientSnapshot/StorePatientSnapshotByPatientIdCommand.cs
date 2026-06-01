using System.Text.Json.Serialization;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Patients.StorePatientSnapshot;

[method: JsonConstructor]
public class StorePatientSnapshotByPatientIdCommand(
    Guid id,
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email,
    DateTime registeredAt
) : InternalCommandBase(id)
{
    public Guid PatientId { get; } = patientId;
    public string FullName { get; } = fullName;
    public DateTime DateOfBirth { get; } = dateOfBirth;
    public string? Gender { get; } = gender;
    public string? MothersFullName { get; } = mothersFullName;
    public string? DocumentId { get; } = documentId;
    public string? Phone { get; } = phone;
    public string? Email { get; } = email;
    public DateTime RegisteredAt { get; } = registeredAt;
}
