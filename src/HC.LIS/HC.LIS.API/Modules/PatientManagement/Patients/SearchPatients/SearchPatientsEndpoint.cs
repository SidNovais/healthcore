using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.SearchPatients;

namespace HC.LIS.API.Modules.PatientManagement.Patients.SearchPatients;

internal static class SearchPatientsEndpoint
{
    internal static async Task<IResult> Handle(
        string search,
        IPatientManagementModule module,
        CancellationToken ct)
    {
        var results = await module.ExecuteQueryAsync(
            new SearchPatientsQuery($"%{search}%")).ConfigureAwait(false);
        return TypedResults.Ok(results);
    }
}
