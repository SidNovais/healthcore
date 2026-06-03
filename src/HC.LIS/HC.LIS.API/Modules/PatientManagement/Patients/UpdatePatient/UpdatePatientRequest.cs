namespace HC.LIS.API.Modules.PatientManagement.Patients.UpdatePatient;

internal sealed record UpdatePatientRequest(
    string FullName,
    DateTime DateOfBirth,
    string? Gender,
    string? MothersFullName,
    string? DocumentId,
    string? Phone,
    string? Email);
