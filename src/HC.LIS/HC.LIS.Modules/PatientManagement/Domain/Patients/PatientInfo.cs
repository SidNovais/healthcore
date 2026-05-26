using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients;

public class PatientInfo : ValueObject
{
    public string FullName { get; }
    public DateTime DateOfBirth { get; }
    public string? Gender { get; }
    public string? MothersFullName { get; }
    public string? DocumentId { get; }
    public string? Phone { get; }
    public string? Email { get; }

    private PatientInfo(
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email
    )
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        MothersFullName = mothersFullName;
        DocumentId = documentId;
        Phone = phone;
        Email = email;
    }

    public static PatientInfo Of(
        string fullName,
        DateTime dateOfBirth,
        string? gender = null,
        string? mothersFullName = null,
        string? documentId = null,
        string? phone = null,
        string? email = null
    ) => new(fullName, dateOfBirth, gender, mothersFullName, documentId, phone, email);

    public static PatientInfo Anonymized() => new(
        "ANONYMIZED",
        new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        "ANONYMIZED",
        "ANONYMIZED",
        "ANONYMIZED",
        "ANONYMIZED",
        "ANONYMIZED"
    );
}
