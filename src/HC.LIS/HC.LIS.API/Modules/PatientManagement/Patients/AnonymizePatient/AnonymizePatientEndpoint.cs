using HC.Core.Domain;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

namespace HC.LIS.API.Modules.PatientManagement.Patients.AnonymizePatient;

internal static class AnonymizePatientEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        IPatientManagementModule module,
        CancellationToken ct)
    {
        await module.ExecuteCommandAsync(
            new AnonymizePatientCommand(id, SystemClock.Now)).ConfigureAwait(false);
        return TypedResults.NoContent();
    }
}
