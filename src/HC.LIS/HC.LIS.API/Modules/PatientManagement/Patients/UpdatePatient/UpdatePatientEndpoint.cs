using HC.Core.Domain;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

namespace HC.LIS.API.Modules.PatientManagement.Patients.UpdatePatient;

internal static class UpdatePatientEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        UpdatePatientRequest request,
        IPatientManagementModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(new UpdatePatientCommand(
            id,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.MothersFullName,
            request.DocumentId,
            request.Phone,
            request.Email,
            SystemClock.Now)).ConfigureAwait(false);

        return TypedResults.NoContent();
    }
}
