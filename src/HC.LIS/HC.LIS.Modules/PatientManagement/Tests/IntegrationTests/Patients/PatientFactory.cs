using System.Threading.Tasks;
using HC.Core.Domain;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

namespace HC.LIS.Modules.PatientManagement.IntegrationTests.Patients;

internal static class PatientFactory
{
    public static async Task CreateAsync(IPatientManagementModule patientManagementModule)
    {
        await patientManagementModule.ExecuteCommandAsync(
            new RegisterPatientCommand(
                PatientSampleData.PatientId,
                PatientSampleData.FullName,
                PatientSampleData.DateOfBirth,
                PatientSampleData.Gender,
                PatientSampleData.MothersFullName,
                PatientSampleData.DocumentId,
                PatientSampleData.Phone,
                PatientSampleData.Email,
                SystemClock.Now
            )
        ).ConfigureAwait(false);
    }
}
