using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;

namespace HC.LIS.API.Modules.PatientManagement.Patients.GetPatientDetails;

internal static class GetPatientDetailsEndpoint
{
    internal static async Task<IResult> Handle(
        Guid id,
        IPatientManagementModule module,
        CancellationToken ct)
    {
        var patient = await module.ExecuteQueryAsync(
            new GetPatientDetailsQuery(id)).ConfigureAwait(false);
        return patient is null ? TypedResults.NotFound() : TypedResults.Ok(patient);
    }
}
