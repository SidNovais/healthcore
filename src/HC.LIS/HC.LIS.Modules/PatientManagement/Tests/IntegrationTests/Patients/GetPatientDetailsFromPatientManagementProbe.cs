using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;

namespace HC.LIS.Modules.PatientManagement.IntegrationTests.Patients;

public class GetPatientDetailsFromPatientManagementProbe(
    Guid expectedPatientId,
    IPatientManagementModule patientManagementModule,
    Func<PatientDetailsDto?, bool>? satisfiedWhen = null
) : IProbe<PatientDetailsDto>
{
    private readonly Guid _expectedPatientId = expectedPatientId;
    private readonly IPatientManagementModule _patientManagementModule = patientManagementModule;
    private readonly Func<PatientDetailsDto?, bool> _satisfiedWhen = satisfiedWhen ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"PatientDetails not found or unsatisfied for {_expectedPatientId}";

    public async Task<PatientDetailsDto?> GetSampleAsync()
    {
        return await _patientManagementModule
            .ExecuteQueryAsync(new GetPatientDetailsQuery(_expectedPatientId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(PatientDetailsDto? sample) => _satisfiedWhen(sample);
}
