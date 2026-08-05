using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.UpdatePhysicianSnapshot;

internal class UpdatePhysicianSnapshotByPhysicianIdCommandHandler(
    IPhysicianSnapshotRepository physicianSnapshotRepository
) : ICommandHandler<UpdatePhysicianSnapshotByPhysicianIdCommand>
{
    private readonly IPhysicianSnapshotRepository _physicianSnapshotRepository = physicianSnapshotRepository;

    public async Task Handle(
        UpdatePhysicianSnapshotByPhysicianIdCommand command,
        CancellationToken cancellationToken)
    {
        await _physicianSnapshotRepository.UpdateAsync(
            command.PhysicianId,
            command.FullName,
            command.LicenceNumber,
            command.UpdatedAt
        ).ConfigureAwait(false);
    }
}
