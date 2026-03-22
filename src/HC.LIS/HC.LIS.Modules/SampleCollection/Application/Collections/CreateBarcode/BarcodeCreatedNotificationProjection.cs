using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;

public class BarcodeCreatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<BarcodeCreatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(BarcodeCreatedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
