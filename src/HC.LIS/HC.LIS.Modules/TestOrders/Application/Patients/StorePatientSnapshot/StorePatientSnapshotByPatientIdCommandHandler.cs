using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Application.Patients.StorePatientSnapshot;

internal class StorePatientSnapshotByPatientIdCommandHandler(
    IPatientSnapshotRepository patientSnapshotRepository
) : ICommandHandler<StorePatientSnapshotByPatientIdCommand>
{
    private readonly IPatientSnapshotRepository _patientSnapshotRepository = patientSnapshotRepository;

    public async Task Handle(
        StorePatientSnapshotByPatientIdCommand command,
        CancellationToken cancellationToken
    )
    {
        await _patientSnapshotRepository.StoreAsync(
            command.PatientId,
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email,
            command.RegisteredAt
        ).ConfigureAwait(false);
    }
}
