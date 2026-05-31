using System;

namespace HC.LIS.Modules.PatientManagement.IntegrationTests.Patients;

public readonly struct PatientSampleData
{
    public static readonly Guid PatientId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e40");
    public static readonly string FullName = "John Doe";
    public static readonly DateTime DateOfBirth = new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    public static readonly string Gender = "Male";
    public static readonly string MothersFullName = "Jane Doe";
    public static readonly string DocumentId = "DOC123456";
    public static readonly string Phone = "+1234567890";
    public static readonly string Email = "john.doe@example.com";

    public static readonly string UpdatedFullName = "Jane Smith";
    public static readonly DateTime UpdatedDateOfBirth = new DateTime(1985, 6, 20, 0, 0, 0, DateTimeKind.Utc);
    public static readonly string UpdatedGender = "Female";
    public static readonly string UpdatedMothersFullName = "Mary Smith";
    public static readonly string UpdatedDocumentId = "DOC999999";
    public static readonly string UpdatedPhone = "+9876543210";
    public static readonly string UpdatedEmail = "jane.smith@example.com";
}
