using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class RegisterPatientCommand(
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email,
    DateTime registeredAt
) : CommandBase<Guid>
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
