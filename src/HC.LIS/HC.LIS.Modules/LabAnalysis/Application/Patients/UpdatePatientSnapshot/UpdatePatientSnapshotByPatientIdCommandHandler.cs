using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;

internal class UpdatePatientSnapshotByPatientIdCommandHandler(
    IPatientSnapshotRepository patientSnapshotRepository
) : ICommandHandler<UpdatePatientSnapshotByPatientIdCommand>
{
    private readonly IPatientSnapshotRepository _patientSnapshotRepository = patientSnapshotRepository;

    public async Task Handle(
        UpdatePatientSnapshotByPatientIdCommand command,
        CancellationToken cancellationToken
    )
    {
        await _patientSnapshotRepository.UpdateAsync(
            command.PatientId,
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email
        ).ConfigureAwait(false);
    }
}
