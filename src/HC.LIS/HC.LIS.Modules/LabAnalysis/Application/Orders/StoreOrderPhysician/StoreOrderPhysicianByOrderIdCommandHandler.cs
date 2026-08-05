using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Orders.StoreOrderPhysician;

internal class StoreOrderPhysicianByOrderIdCommandHandler(
    IOrderPhysicianRepository orderPhysicianRepository
) : ICommandHandler<StoreOrderPhysicianByOrderIdCommand>
{
    private readonly IOrderPhysicianRepository _orderPhysicianRepository = orderPhysicianRepository;

    public async Task Handle(
        StoreOrderPhysicianByOrderIdCommand command,
        CancellationToken cancellationToken)
    {
        await _orderPhysicianRepository.StoreAsync(
            command.OrderId,
            command.PhysicianId,
            command.RequestedAt
        ).ConfigureAwait(false);
    }
}
