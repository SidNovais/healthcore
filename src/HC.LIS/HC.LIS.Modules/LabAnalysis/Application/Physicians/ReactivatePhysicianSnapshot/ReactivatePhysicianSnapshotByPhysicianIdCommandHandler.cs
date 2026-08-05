using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.ReactivatePhysicianSnapshot;

internal class ReactivatePhysicianSnapshotByPhysicianIdCommandHandler(
    IPhysicianSnapshotRepository physicianSnapshotRepository
) : ICommandHandler<ReactivatePhysicianSnapshotByPhysicianIdCommand>
{
    private readonly IPhysicianSnapshotRepository _physicianSnapshotRepository = physicianSnapshotRepository;

    public async Task Handle(
        ReactivatePhysicianSnapshotByPhysicianIdCommand command,
        CancellationToken cancellationToken)
    {
        await _physicianSnapshotRepository.ReactivateAsync(
            command.PhysicianId,
            command.ReactivatedAt
        ).ConfigureAwait(false);
    }
}
