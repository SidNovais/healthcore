using HC.Core.Domain;
using HC.LIS.API.Common;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

namespace HC.LIS.API.Modules.PatientManagement.Patients.RegisterPatient;

internal static class RegisterPatientEndpoint
{
    internal static async Task<IResult> Handle(
        RegisterPatientRequest request,
        IPatientManagementModule module,
        CancellationToken ct)
    {
        var patientId = Guid.CreateVersion7();
        await module.ExecuteCommandAsync(new RegisterPatientCommand(
            patientId,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.MothersFullName,
            request.DocumentId,
            request.Phone,
            request.Email,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.Created($"/api/v1/patients/{patientId}", new CreatedIdResponse(patientId));
    }
}
