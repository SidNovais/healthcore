using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Application.Projections;
using MediatR;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class AllSamplesCollectedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<AllSamplesCollectedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        AllSamplesCollectedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
