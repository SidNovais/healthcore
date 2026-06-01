using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Patients.AnonymizePatientSnapshot;

internal class AnonymizePatientSnapshotByPatientIdCommandHandler(
    IPatientSnapshotRepository patientSnapshotRepository
) : ICommandHandler<AnonymizePatientSnapshotByPatientIdCommand>
{
    private readonly IPatientSnapshotRepository _patientSnapshotRepository = patientSnapshotRepository;

    public async Task Handle(
        AnonymizePatientSnapshotByPatientIdCommand command,
        CancellationToken cancellationToken
    )
    {
        await _patientSnapshotRepository.AnonymizeAsync(
            command.PatientId,
            command.AnonymizedAt
        ).ConfigureAwait(false);
    }
}
