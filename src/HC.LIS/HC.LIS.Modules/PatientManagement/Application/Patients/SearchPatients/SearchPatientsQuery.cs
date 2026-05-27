using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.SearchPatients;

public class SearchPatientsQuery(string searchTerm) : QueryBase<IReadOnlyCollection<PatientSearchResultDto>>
{
    public string SearchTerm { get; } = searchTerm;
}
