using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.UnitTests.Patients;

internal static class PatientFactory
{
    public static Patient Create() => Patient.Register(
        PatientSampleData.PatientId,
        PatientSampleData.FullName,
        PatientSampleData.DateOfBirth,
        PatientSampleData.Gender,
        PatientSampleData.MothersFullName,
        PatientSampleData.DocumentId,
        PatientSampleData.Phone,
        PatientSampleData.Email,
        PatientSampleData.RegisteredAt
    );
}
