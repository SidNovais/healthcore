namespace HC.LIS.API.Modules.PatientManagement.Patients.RegisterPatient;

internal sealed record RegisterPatientRequest(
    string FullName,
    DateTime DateOfBirth,
    string? Gender,
    string? MothersFullName,
    string? DocumentId,
    string? Phone,
    string? Email);
