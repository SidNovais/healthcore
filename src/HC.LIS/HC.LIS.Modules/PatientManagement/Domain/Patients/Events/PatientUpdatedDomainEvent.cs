using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

public class PatientUpdatedDomainEvent(
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email,
    DateTime updatedAt
) : DomainEvent
{
    public Guid PatientId { get; } = patientId;
    public string FullName { get; } = fullName;
    public DateTime DateOfBirth { get; } = dateOfBirth;
    public string? Gender { get; } = gender;
    public string? MothersFullName { get; } = mothersFullName;
    public string? DocumentId { get; } = documentId;
    public string? Phone { get; } = phone;
    public string? Email { get; } = email;
    public DateTime UpdatedAt { get; } = updatedAt;
}
